namespace Foldora.Core.Menu;

/// <summary>
/// Настройки пользовательского меню создания папки.
/// </summary>
public sealed class FolderMenuSettings
{
    public string Title { get; init; } = FolderMenuDefaultTitles.Russian;

    public bool TitleIsCustom { get; init; }

    public List<FolderMenuEntry> Entries { get; init; } = [];

    public static FolderMenuSettings CreateDefault(string language = Foldora.Core.Settings.FoldoraLanguage.Russian)
    {
        return new FolderMenuSettings
        {
            Title = FolderMenuDefaultTitles.GetForLanguage(language),
            TitleIsCustom = false
        };
    }
}
