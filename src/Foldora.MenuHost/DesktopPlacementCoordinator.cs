using Foldora.Shell.Desktop;

namespace Foldora.MenuHost;

internal sealed class DesktopPlacementCoordinator
{
    private readonly IDesktopTargetDetector targetDetector;
    private readonly IDesktopIconPositioningService positioningService;
    private readonly IPlacementRetryDelay retryDelay;
    private readonly DesktopPlacementRetryPolicy retryPolicy;

    public DesktopPlacementCoordinator(
        IDesktopTargetDetector targetDetector,
        IDesktopIconPositioningService positioningService,
        IPlacementRetryDelay? retryDelay = null,
        DesktopPlacementRetryPolicy? retryPolicy = null)
    {
        ArgumentNullException.ThrowIfNull(targetDetector);
        ArgumentNullException.ThrowIfNull(positioningService);
        this.targetDetector = targetDetector;
        this.positioningService = positioningService;
        this.retryDelay = retryDelay ?? new PlacementRetryDelay();
        this.retryPolicy = retryPolicy ?? DesktopPlacementRetryPolicy.Default;
    }

    public async Task<DesktopPlacementResult> TryPlaceCreatedFolderAsync(
        string targetDirectory,
        string createdFolderPath,
        CursorPosition? capturedCursorPosition,
        CancellationToken cancellationToken = default)
    {
        var detection = targetDetector.Detect(targetDirectory);
        if (!detection.IsDesktopTarget)
        {
            return DesktopPlacementResult.Skipped(
                detection,
                ResolveFolderName(createdFolderPath),
                "Target is not Desktop.");
        }

        if (capturedCursorPosition is null)
        {
            return DesktopPlacementResult.Skipped(
                detection,
                ResolveFolderName(createdFolderPath),
                "Cursor position was not captured.");
        }

        var folderName = ResolveFolderName(createdFolderPath);
        if (string.IsNullOrWhiteSpace(folderName))
        {
            return DesktopPlacementResult.Failed(
                detection,
                folderName,
                0,
                DesktopIconCoordinateSpace.Screen,
                "Created folder name could not be resolved.");
        }

        DesktopIconPositioningResult? result = null;
        for (var attempt = 1; attempt <= retryPolicy.MaxAttempts; attempt++)
        {
            try
            {
                result = positioningService.TryPositionByName(
                    folderName,
                    capturedCursorPosition.Value.X,
                    capturedCursorPosition.Value.Y,
                    DesktopIconCoordinateSpace.Screen);
            }
            catch (Exception exception) when (exception is InvalidOperationException
                                             or ArgumentException
                                             or UnauthorizedAccessException
                                             or IOException
                                             or System.Security.SecurityException)
            {
                return DesktopPlacementResult.Failed(
                    detection,
                    folderName,
                    attempt,
                    DesktopIconCoordinateSpace.Screen,
                    exception.Message,
                    exception.GetType().FullName,
                    exception.Message);
            }

            if (result.Success)
            {
                return DesktopPlacementResult.Succeeded(
                    detection,
                    folderName,
                    attempt,
                    DesktopIconCoordinateSpace.Screen,
                    result.Message);
            }

            if (!IsItemNotFound(result) || attempt >= retryPolicy.MaxAttempts)
            {
                return DesktopPlacementResult.Failed(
                    detection,
                    folderName,
                    attempt,
                    DesktopIconCoordinateSpace.Screen,
                    result.Message);
            }

            await retryDelay.DelayAsync(retryPolicy.Delay, cancellationToken);
        }

        return DesktopPlacementResult.Failed(
            detection,
            folderName,
            retryPolicy.MaxAttempts,
            DesktopIconCoordinateSpace.Screen,
            result?.Message ?? "Positioning retry exhausted.");
    }

    private static bool IsItemNotFound(DesktopIconPositioningResult result)
    {
        return !result.Success
               && result.Message.StartsWith("Desktop item was not found:", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ResolveFolderName(string createdFolderPath)
    {
        return string.IsNullOrWhiteSpace(createdFolderPath)
            ? null
            : Path.GetFileName(Path.TrimEndingDirectorySeparator(createdFolderPath));
    }
}

internal sealed record DesktopPlacementResult(
    string Status,
    DesktopTargetDetectionResult TargetDetection,
    string? CreatedFolderName,
    bool PositioningAttempted,
    int AttemptCount,
    DesktopIconCoordinateSpace? CoordinateSpaceUsed,
    string Message,
    string? ExceptionType = null,
    string? ExceptionMessage = null)
{
    public static DesktopPlacementResult Skipped(
        DesktopTargetDetectionResult targetDetection,
        string? createdFolderName,
        string message)
    {
        return new DesktopPlacementResult(
            "skipped",
            targetDetection,
            createdFolderName,
            false,
            0,
            null,
            message);
    }

    public static DesktopPlacementResult Succeeded(
        DesktopTargetDetectionResult targetDetection,
        string createdFolderName,
        int attemptCount,
        DesktopIconCoordinateSpace coordinateSpace,
        string message)
    {
        return new DesktopPlacementResult(
            "success",
            targetDetection,
            createdFolderName,
            true,
            attemptCount,
            coordinateSpace,
            message);
    }

    public static DesktopPlacementResult Failed(
        DesktopTargetDetectionResult targetDetection,
        string? createdFolderName,
        int attemptCount,
        DesktopIconCoordinateSpace coordinateSpace,
        string message,
        string? exceptionType = null,
        string? exceptionMessage = null)
    {
        return new DesktopPlacementResult(
            "failure",
            targetDetection,
            createdFolderName,
            attemptCount > 0,
            attemptCount,
            coordinateSpace,
            message,
            exceptionType,
            exceptionMessage);
    }
}
