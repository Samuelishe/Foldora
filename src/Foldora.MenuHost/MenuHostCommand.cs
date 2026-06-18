namespace Foldora.MenuHost;

internal enum MenuHostCommandKind
{
    Create,
    Apply,
    Unknown
}

internal sealed record MenuHostCommand(
    MenuHostCommandKind Kind,
    string? TargetPath = null,
    string? FolderPath = null,
    string? EntryId = null,
    bool IsValid = true)
{
    public static MenuHostCommand Invalid(MenuHostCommandKind kind)
    {
        return new MenuHostCommand(kind, IsValid: false);
    }
}
