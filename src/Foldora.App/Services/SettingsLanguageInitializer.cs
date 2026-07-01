using Foldora.Core.Settings;

namespace Foldora.App.Services;

/// <summary>
/// Делает одноразовый first-run выбор языка и сохраняет его в settings.
/// </summary>
public sealed class SettingsLanguageInitializer : ISettingsLanguageInitializer
{
    private readonly FoldoraSettingsStorage storage;
    private readonly ISystemLanguageProvider systemLanguageProvider;

    public SettingsLanguageInitializer(
        FoldoraSettingsStorage storage,
        ISystemLanguageProvider systemLanguageProvider)
    {
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.systemLanguageProvider = systemLanguageProvider ?? throw new ArgumentNullException(nameof(systemLanguageProvider));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var loadResult = await storage.LoadWithLanguageMetadataAsync(
            createSettingsIfMissing: false,
            cancellationToken);

        if (loadResult.LanguageWasPersisted && loadResult.LanguageWasSupported)
        {
            return;
        }

        var selectedLanguage = loadResult.LanguageWasPersisted
            ? FoldoraLanguage.English
            : DetectStartupLanguage(systemLanguageProvider.CurrentUiCultureName);

        await storage.SaveAsync(loadResult.Settings with { Language = selectedLanguage }, cancellationToken);
    }

    public static string DetectStartupLanguage(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return FoldoraLanguage.English;
        }

        var normalized = cultureName.Trim().Replace('_', '-');

        if (MatchesLanguage(normalized, FoldoraLanguage.Russian))
        {
            return FoldoraLanguage.Russian;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.English))
        {
            return FoldoraLanguage.English;
        }

        if (IsTraditionalChineseCulture(normalized))
        {
            return FoldoraLanguage.TraditionalChinese;
        }

        if (IsSimplifiedChineseCulture(normalized))
        {
            return FoldoraLanguage.SimplifiedChinese;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.German))
        {
            return FoldoraLanguage.German;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Spanish))
        {
            return FoldoraLanguage.Spanish;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.French))
        {
            return FoldoraLanguage.French;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Japanese))
        {
            return FoldoraLanguage.Japanese;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.BrazilianPortuguese))
        {
            return FoldoraLanguage.BrazilianPortuguese;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.PortuguesePortugal))
        {
            return FoldoraLanguage.PortuguesePortugal;
        }

        if (MatchesLanguage(normalized, "pt"))
        {
            return FoldoraLanguage.BrazilianPortuguese;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Korean))
        {
            return FoldoraLanguage.Korean;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Ukrainian))
        {
            return FoldoraLanguage.Ukrainian;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Polish))
        {
            return FoldoraLanguage.Polish;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Turkish))
        {
            return FoldoraLanguage.Turkish;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Romanian))
        {
            return FoldoraLanguage.Romanian;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Czech))
        {
            return FoldoraLanguage.Czech;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Hungarian))
        {
            return FoldoraLanguage.Hungarian;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Bulgarian))
        {
            return FoldoraLanguage.Bulgarian;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Italian))
        {
            return FoldoraLanguage.Italian;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Dutch))
        {
            return FoldoraLanguage.Dutch;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Indonesian))
        {
            return FoldoraLanguage.Indonesian;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Vietnamese))
        {
            return FoldoraLanguage.Vietnamese;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Hindi))
        {
            return FoldoraLanguage.Hindi;
        }

        if (MatchesLanguage(normalized, FoldoraLanguage.Thai))
        {
            return FoldoraLanguage.Thai;
        }

        return FoldoraLanguage.English;
    }

    private static bool MatchesLanguage(string cultureName, string language)
    {
        return cultureName.Equals(language, StringComparison.OrdinalIgnoreCase)
               || cultureName.StartsWith(language + "-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSimplifiedChineseCulture(string cultureName)
    {
        return cultureName.Equals("zh", StringComparison.OrdinalIgnoreCase)
               || cultureName.Equals("zh-CN", StringComparison.OrdinalIgnoreCase)
               || cultureName.Equals("zh-SG", StringComparison.OrdinalIgnoreCase)
               || cultureName.Equals(FoldoraLanguage.SimplifiedChinese, StringComparison.OrdinalIgnoreCase)
               || cultureName.StartsWith(FoldoraLanguage.SimplifiedChinese + "-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTraditionalChineseCulture(string cultureName)
    {
        return cultureName.Equals("zh-TW", StringComparison.OrdinalIgnoreCase)
               || cultureName.Equals("zh-HK", StringComparison.OrdinalIgnoreCase)
               || cultureName.Equals("zh-MO", StringComparison.OrdinalIgnoreCase)
               || cultureName.Equals(FoldoraLanguage.TraditionalChinese, StringComparison.OrdinalIgnoreCase)
               || cultureName.StartsWith(FoldoraLanguage.TraditionalChinese + "-", StringComparison.OrdinalIgnoreCase);
    }
}
