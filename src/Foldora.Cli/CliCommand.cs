namespace Foldora.Cli;

public enum CliCommandKind
{
    Help,
    Apply,
    Create,
    Clear,
    MenuList,
    MenuAdd,
    MenuRemove,
    MenuReset,
    ConvertIcon,
    Skeleton,
    RegisterMenu,
    UnregisterMenu,
    DiagnosticsDesktopIniPolicy,
    DiagnosticsDesktopIconPosition,
    Quote,
    Unknown
}

public sealed record CliCommand(
    CliCommandKind Kind,
    string Name,
    string? FolderPath = null,
    string? TargetPath = null,
    string? InputPath = null,
    string? OutputPath = null,
    string? IconPath = null,
    string? DisplayName = null,
    string? DefaultFolderName = null,
    string? GroupName = null,
    string? EntryId = null,
    string? CliExecutablePath = null,
    string? CommandHostPath = null,
    bool DryRun = false,
    bool Yes = false,
    bool Force = false,
    int? X = null,
    int? Y = null,
    string? CoordinateSpace = null,
    string? QuoteValue = null,
    string? Error = null)
{
    public bool IsValid => Error is null;
}
