namespace Foldora.App.Services;

public sealed record SettingsDialogResult(
    bool Changed,
    string Language,
    bool MenuStateChanged = false);
