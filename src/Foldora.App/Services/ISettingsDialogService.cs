namespace Foldora.App.Services;

public interface ISettingsDialogService
{
    Task<SettingsDialogResult> ShowSettingsAsync();
}
