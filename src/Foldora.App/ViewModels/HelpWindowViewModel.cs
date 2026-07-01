using System.Reflection;
using Foldora.App.Services;

namespace Foldora.App.ViewModels;

/// <summary>
/// ViewModel окна краткой справки Foldora.
/// </summary>
public sealed class HelpWindowViewModel
{
    private readonly ILocalizationService localizationService;
    private readonly string versionText;

    public HelpWindowViewModel(ILocalizationService localizationService)
    {
        this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        versionText = CreateVersionText();
    }

    public LocalizationResources L => localizationService.Resources;

    public string VersionText => versionText;

    public bool HasVersion => !string.IsNullOrWhiteSpace(VersionText);

    private string CreateVersionText()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        return string.IsNullOrWhiteSpace(version)
            ? string.Empty
            : string.Format(L.HelpVersionFormat, version);
    }
}
