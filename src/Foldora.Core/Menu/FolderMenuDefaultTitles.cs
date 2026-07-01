using Foldora.Core.Settings;

namespace Foldora.Core.Menu;

/// <summary>
/// Compatibility-набор известных product default titles для режима локализованного default title.
/// </summary>
public static class FolderMenuDefaultTitles
{
    public const string Russian = "Создать папку";

    public const string English = "Create folder";

    public static string GetForLanguage(string language)
    {
        return FoldoraLanguage.NormalizeOrDefault(language) == FoldoraLanguage.English
            ? English
            : Russian;
    }

    public static bool IsKnownDefault(string? title)
    {
        var normalized = Normalize(title);
        return string.IsNullOrEmpty(normalized)
               || string.Equals(normalized, Russian, StringComparison.Ordinal)
               || string.Equals(normalized, English, StringComparison.Ordinal);
    }

    public static string Normalize(string? title)
    {
        return string.IsNullOrWhiteSpace(title) ? string.Empty : title.Trim();
    }
}
