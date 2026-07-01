namespace Foldora.Core.Validation;

/// <summary>
/// Одна проблема валидации пользовательского меню.
/// </summary>
public sealed record FolderMenuValidationIssue
{
    private static readonly IReadOnlyDictionary<string, string> EmptyParameters =
        new Dictionary<string, string>();

    public FolderMenuValidationIssue(
        FolderMenuValidationSeverity severity,
        string message,
        string? code = null,
        string? entryId = null,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        Severity = severity;
        Message = message;
        Code = code;
        EntryId = entryId;
        Parameters = parameters ?? EmptyParameters;
    }

    public FolderMenuValidationSeverity Severity { get; init; }

    public string Message { get; init; }

    public string? Code { get; init; }

    public string? EntryId { get; init; }

    public IReadOnlyDictionary<string, string> Parameters { get; init; }
}
