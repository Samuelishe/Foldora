namespace Foldora.Core.Settings;

/// <summary>
/// Поддерживаемые языки интерфейса Foldora.
/// </summary>
public static class FoldoraLanguage
{
    public const string Russian = "ru";
    public const string English = "en";

    public static string NormalizeOrDefault(string? language)
    {
        if (string.Equals(language, Russian, StringComparison.OrdinalIgnoreCase))
        {
            return Russian;
        }

        return English;
    }

    public static bool IsSupported(string? language)
    {
        return string.Equals(language, Russian, StringComparison.OrdinalIgnoreCase)
               || string.Equals(language, English, StringComparison.OrdinalIgnoreCase);
    }
}
