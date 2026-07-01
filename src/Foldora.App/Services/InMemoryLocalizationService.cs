using Foldora.App.ViewModels;
using Foldora.Core.Settings;
using System.Reflection;
using System.Text.Json;

namespace Foldora.App.Services;

/// <summary>
/// Минимальный catalog-backed слой локализации для WPF.
/// </summary>
public sealed class InMemoryLocalizationService : ILocalizationService
{
    private static readonly Assembly ResourceAssembly = typeof(InMemoryLocalizationService).Assembly;
    private static readonly IReadOnlyDictionary<string, string> FallbackStrings = LoadCatalog(FoldoraLanguage.Russian);

    public InMemoryLocalizationService(string language = FoldoraLanguage.Russian)
    {
        Resources = new LocalizationResources();
        SetLanguage(language);
    }

    public string CurrentLanguage { get; private set; } = FoldoraLanguage.Russian;

    public LocalizationResources Resources { get; }

    public void SetLanguage(string language)
    {
        CurrentLanguage = FoldoraLanguage.NormalizeOrDefault(language);
        Resources.Apply(CreateStrings(CurrentLanguage));
    }

    private static IReadOnlyDictionary<string, string> CreateStrings(string language)
    {
        var catalog = LoadCatalog(language);
        if (language == FoldoraLanguage.Russian)
        {
            return catalog;
        }

        return FallbackStrings
            .Concat(catalog)
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);
    }

    public static IReadOnlyDictionary<string, string> LoadCatalog(string language)
    {
        var normalized = FoldoraLanguage.NormalizeOrDefault(language);
        var resourceName = $"Foldora.App.Localization.{normalized}.json";
        using var stream = ResourceAssembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Localization catalog was not found: {resourceName}");
        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
            ?? throw new InvalidOperationException($"Localization catalog is empty: {resourceName}");
        return values;
    }
}
