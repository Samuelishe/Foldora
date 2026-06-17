namespace Foldora.Cli;

public enum CliCommandKind
{
    Help,
    Apply,
    Clear,
    MenuList,
    MenuAdd,
    MenuRemove,
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
    string? IconPath = null,
    string? DisplayName = null,
    string? EntryId = null,
    string? QuoteValue = null,
    string? Error = null)
{
    public bool IsValid => Error is null;
}
