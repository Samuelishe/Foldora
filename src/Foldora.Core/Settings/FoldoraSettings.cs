namespace Foldora.Core.Settings;

/// <summary>
/// Пользовательские настройки Foldora, хранящиеся в AppData.
/// </summary>
public sealed record FoldoraSettings
{
    public string? ActivePackId { get; init; }

    public string Language { get; init; } = "ru";

    public bool ExplorerIntegrationEnabled { get; init; }

    public string? DefaultCreateStyleId { get; init; }

    public string? DefaultApplyStyleId { get; init; }

    public bool ShowLegacyContextMenu { get; init; } = true;

    public bool OpenPickerForCustomStyle { get; init; } = true;
}
