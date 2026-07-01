namespace Foldora.Core.Settings;

/// <summary>
/// Результат загрузки settings вместе с metadata о persisted language.
/// </summary>
public sealed record FoldoraSettingsLoadResult(
    FoldoraSettings Settings,
    bool LanguageWasPersisted,
    bool LanguageWasSupported);
