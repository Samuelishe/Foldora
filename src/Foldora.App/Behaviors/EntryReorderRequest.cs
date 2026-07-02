namespace Foldora.App.Behaviors;

/// <summary>
/// Запрос ViewModel на переупорядочивание пункта меню.
/// </summary>
public sealed record EntryReorderRequest(
    string SourceEntryId,
    string TargetEntryId,
    EntryReorderDropPosition DropPosition);
