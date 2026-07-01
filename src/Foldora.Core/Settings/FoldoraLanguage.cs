namespace Foldora.Core.Settings;

/// <summary>
/// Поддерживаемые языки интерфейса Foldora.
/// </summary>
public static class FoldoraLanguage
{
    public const string Russian = "ru";
    public const string English = "en";
    public const string SimplifiedChinese = "zh-Hans";
    public const string German = "de";
    public const string Spanish = "es";
    public const string French = "fr";
    public const string Japanese = "ja";
    public const string BrazilianPortuguese = "pt-BR";
    public const string Korean = "ko";
    public const string Ukrainian = "uk";
    public const string Polish = "pl";
    public const string Turkish = "tr";
    public const string Romanian = "ro";
    public const string Czech = "cs";
    public const string Hungarian = "hu";
    public const string Bulgarian = "bg";

    private static readonly string[] SupportedLocaleValues =
    [
        Russian,
        English,
        SimplifiedChinese,
        German,
        Spanish,
        French,
        Japanese,
        BrazilianPortuguese,
        Korean,
        Ukrainian,
        Polish,
        Turkish,
        Romanian,
        Czech,
        Hungarian,
        Bulgarian
    ];

    public static IReadOnlyList<string> SupportedLocales => SupportedLocaleValues;

    public static string NormalizeOrDefault(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return English;
        }

        foreach (var supportedLocale in SupportedLocaleValues)
        {
            if (string.Equals(language, supportedLocale, StringComparison.OrdinalIgnoreCase))
            {
                return supportedLocale;
            }
        }

        return English;
    }

    public static bool IsSupported(string? language)
    {
        return !string.IsNullOrWhiteSpace(language)
               && SupportedLocaleValues.Any(supportedLocale =>
                   string.Equals(language, supportedLocale, StringComparison.OrdinalIgnoreCase));
    }
}
