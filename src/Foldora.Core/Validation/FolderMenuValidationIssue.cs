namespace Foldora.Core.Validation;

/// <summary>
/// Одна проблема валидации пользовательского меню.
/// </summary>
public sealed record FolderMenuValidationIssue(
    FolderMenuValidationSeverity Severity,
    string Message,
    string? Code = null,
    string? EntryId = null);
