namespace Foldora.Core.Menu;

/// <summary>
/// Настройки пользовательского меню создания папки.
/// </summary>
public sealed class FolderMenuSettings
{
    public string Title { get; init; } = "Создать папку";

    public List<FolderMenuEntry> Entries { get; init; } = [];

    public static FolderMenuSettings CreateDefault()
    {
        return new FolderMenuSettings();
    }
}
