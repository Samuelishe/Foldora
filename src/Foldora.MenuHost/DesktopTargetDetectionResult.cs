namespace Foldora.MenuHost;

internal sealed record DesktopTargetDetectionResult(
    string TargetPath,
    string? NormalizedTargetPath,
    string? DesktopPath,
    string? NormalizedDesktopPath,
    bool IsDesktopTarget,
    string? FailureReason);
