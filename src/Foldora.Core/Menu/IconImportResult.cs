namespace Foldora.Core.Menu;

/// <summary>
/// Результат копирования пользовательской иконки в AppData.
/// </summary>
public sealed record IconImportResult(string EntryId, string ImportedIconPath);
