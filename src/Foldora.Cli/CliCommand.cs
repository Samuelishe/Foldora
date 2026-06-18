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
    Skeleton,
    RegisterMenu,
    UnregisterMenu,
    Quote,
    Unknown
}

public sealed record CliCommand(
    CliCommandKind Kind,
    string Name,
    string? FolderPath = null,
    string? TargetPath = null,
    string? IconPath = null,
    string? DisplayName = null,
    string? DefaultFolderName = null,
    string? EntryId = null,
    string? CliExecutablePath = null,
    bool DryRun = false,
    bool Yes = false,
    string? QuoteValue = null,
    string? Error = null)
{
    public bool IsValid => Error is null;
}
