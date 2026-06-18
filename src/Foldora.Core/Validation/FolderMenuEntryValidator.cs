using Foldora.Core.Menu;

namespace Foldora.Core.Validation;

/// <summary>
/// Проверяет один пользовательский пункт меню.
/// </summary>
public sealed class FolderMenuEntryValidator
{
    public FolderMenuValidationResult Validate(FolderMenuEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var issues = new List<FolderMenuValidationIssue>();
        issues.AddRange(DisplayNameValidator.ValidateResolved(entry.DisplayName, entry.Id).Issues);
        issues.AddRange(FolderNameValidator.Validate(entry.DefaultFolderName).Issues.Select(
            issue => issue with { EntryId = entry.Id }));
        issues.AddRange(GroupNameValidator.Validate(entry.GroupName, entry.Id).Issues);

        if (string.IsNullOrWhiteSpace(entry.Id))
        {
            issues.Add(new FolderMenuValidationIssue(
                FolderMenuValidationSeverity.Error,
                "Menu entry id must not be empty.",
                "entry_id_empty",
                entry.Id));
        }

        if (string.IsNullOrWhiteSpace(entry.IconPath))
        {
            issues.Add(new FolderMenuValidationIssue(
                FolderMenuValidationSeverity.Error,
                "Menu entry icon path must not be empty.",
                "entry_icon_path_empty",
                entry.Id));
        }

        return issues.Count == 0 ? FolderMenuValidationResult.Success : new FolderMenuValidationResult(issues);
    }
}
