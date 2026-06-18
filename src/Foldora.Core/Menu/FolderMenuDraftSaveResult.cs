using Foldora.Core.Validation;

namespace Foldora.Core.Menu;

/// <summary>
/// Результат сохранения draft-состояния пользовательского меню.
/// </summary>
public sealed class FolderMenuDraftSaveResult
{
    public FolderMenuDraftSaveResult(bool saved, IReadOnlyList<FolderMenuValidationIssue> issues)
    {
        Saved = saved;
        Issues = issues;
    }

    public bool Saved { get; }

    public IReadOnlyList<FolderMenuValidationIssue> Issues { get; }

    public static FolderMenuDraftSaveResult Success { get; } = new(true, []);

    public static FolderMenuDraftSaveResult Blocked(FolderMenuValidationResult validation)
    {
        ArgumentNullException.ThrowIfNull(validation);

        return new FolderMenuDraftSaveResult(false, validation.Issues);
    }
}
