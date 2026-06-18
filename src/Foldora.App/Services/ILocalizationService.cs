using Foldora.App.ViewModels;

namespace Foldora.App.Services;

public interface ILocalizationService
{
    string CurrentLanguage { get; }

    LocalizationResources Resources { get; }

    void SetLanguage(string language);
}
