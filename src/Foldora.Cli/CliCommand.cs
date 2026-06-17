namespace Foldora.Cli;

public enum CliCommandKind
{
    Help,
    Apply,
    Clear,
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
    string? QuoteValue = null,
    string? Error = null)
{
    public bool IsValid => Error is null;
}
