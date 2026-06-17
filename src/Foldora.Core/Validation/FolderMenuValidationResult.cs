namespace Foldora.Core.Validation;

/// <summary>
/// Результат валидации пользовательского меню.
/// </summary>
public sealed class FolderMenuValidationResult
{
    public FolderMenuValidationResult(IEnumerable<FolderMenuValidationIssue> issues)
    {
        Issues = issues.ToArray();
    }

    public IReadOnlyList<FolderMenuValidationIssue> Issues { get; }

    public bool IsValid => !Issues.Any(issue => issue.Severity == FolderMenuValidationSeverity.Error);

    public static FolderMenuValidationResult Success { get; } = new([]);
}
