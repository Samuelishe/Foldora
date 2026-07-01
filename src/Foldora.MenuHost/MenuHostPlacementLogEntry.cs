using Foldora.Shell.Desktop;

namespace Foldora.MenuHost;

internal sealed class MenuHostPlacementLogEntry
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public string HostPath { get; init; } = AppContext.BaseDirectory;

    public string CommandKind { get; init; } = string.Empty;

    public string? TargetPath { get; set; }

    public string? EntryId { get; set; }

    public string? CreatedFolderPath { get; set; }

    public string? CreatedFolderName { get; set; }

    public string? DesktopDirectoryPath { get; set; }

    public string? NormalizedTargetPath { get; set; }

    public string? NormalizedDesktopPath { get; set; }

    public bool IsDesktopTarget { get; set; }

    public string? DesktopDetectionFailureReason { get; set; }

    public bool CursorCaptured { get; set; }

    public int? CursorX { get; set; }

    public int? CursorY { get; set; }

    public string? CoordinateSpaceUsed { get; set; }

    public bool PositioningAttempted { get; set; }

    public int AttemptCount { get; set; }

    public string FinalPositioningResult { get; set; } = "skipped";

    public string? FinalPositioningMessage { get; set; }

    public string? ExceptionType { get; set; }

    public string? ExceptionMessage { get; set; }

    public int FinalExitCode { get; set; }

    public string Note { get; set; } = "Positioning failure is non-fatal when create succeeded.";

    public void ApplyPlacementResult(DesktopPlacementResult result)
    {
        CreatedFolderName = result.CreatedFolderName;
        DesktopDirectoryPath = result.TargetDetection.DesktopPath;
        NormalizedTargetPath = result.TargetDetection.NormalizedTargetPath;
        NormalizedDesktopPath = result.TargetDetection.NormalizedDesktopPath;
        IsDesktopTarget = result.TargetDetection.IsDesktopTarget;
        DesktopDetectionFailureReason = result.TargetDetection.FailureReason;
        CoordinateSpaceUsed = result.CoordinateSpaceUsed?.ToString();
        PositioningAttempted = result.PositioningAttempted;
        AttemptCount = result.AttemptCount;
        FinalPositioningResult = result.Status;
        FinalPositioningMessage = result.Message;
        ExceptionType = result.ExceptionType;
        ExceptionMessage = result.ExceptionMessage;
    }
}
