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

        if (cultureName.Equals(FoldoraLanguage.Russian, StringComparison.OrdinalIgnoreCase)
            || cultureName.StartsWith(FoldoraLanguage.Russian + "-", StringComparison.OrdinalIgnoreCase))
        {
            return FoldoraLanguage.Russian;
        }

        if (cultureName.Equals(FoldoraLanguage.English, StringComparison.OrdinalIgnoreCase)
            || cultureName.StartsWith(FoldoraLanguage.English + "-", StringComparison.OrdinalIgnoreCase))
        {
            return FoldoraLanguage.English;
        }

        return FoldoraLanguage.English;
    }
}
