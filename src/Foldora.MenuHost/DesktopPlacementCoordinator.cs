using Foldora.Shell.Desktop;

namespace Foldora.MenuHost;

internal sealed class DesktopPlacementCoordinator
{
    private readonly IDesktopTargetDetector targetDetector;
    private readonly IDesktopIconPositioningService positioningService;

    public DesktopPlacementCoordinator(
        IDesktopTargetDetector targetDetector,
        IDesktopIconPositioningService positioningService)
    {
        ArgumentNullException.ThrowIfNull(targetDetector);
        ArgumentNullException.ThrowIfNull(positioningService);
        this.targetDetector = targetDetector;
        this.positioningService = positioningService;
    }

    public DesktopIconPositioningResult? TryPlaceCreatedFolder(
        string targetDirectory,
        string createdFolderPath,
        CursorPosition? capturedCursorPosition)
    {
        if (capturedCursorPosition is null || !targetDetector.IsDesktopDirectory(targetDirectory))
        {
            return null;
        }

        var folderName = Path.GetFileName(Path.TrimEndingDirectorySeparator(createdFolderPath));
        if (string.IsNullOrWhiteSpace(folderName))
        {
            return DesktopIconPositioningResult.Failed("Created folder name could not be resolved.");
        }

        try
        {
            return positioningService.TryPositionByName(
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
            return DesktopIconPositioningResult.Failed(exception.Message);
        }
    }
}
