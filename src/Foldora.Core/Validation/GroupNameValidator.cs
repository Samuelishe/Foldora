namespace Foldora.Core.Validation;

/// <summary>
/// Проверяет имя одноуровневой группы пользовательского меню.
/// </summary>
public static class GroupNameValidator
{
    public const int MaxLength = 80;

    public static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public static FolderMenuValidationResult Validate(string? groupName, string? entryId = null)
    {
        var normalized = Normalize(groupName);
        if (normalized.Length == 0)
        {
            return FolderMenuValidationResult.Success;
        }

        var issues = new List<FolderMenuValidationIssue>();

        if (normalized.Length > MaxLength)
        {
            issues.Add(Error($"Group name must be at most {MaxLength} characters.", "group_name_too_long", entryId));
        }

        if (normalized.Any(char.IsControl))
        {
            issues.Add(Error("Group name must not contain control characters.", "group_name_control_chars", entryId));
        }

        if (normalized.Contains('/') || normalized.Contains('\\'))
        {
            issues.Add(Error("Nested groups are not supported yet. Remove '/' and '\\' from the group name.", "group_name_nested_not_supported", entryId));
        }

        return issues.Count == 0 ? FolderMenuValidationResult.Success : new FolderMenuValidationResult(issues);
    }

    private static FolderMenuValidationIssue Error(string message, string code, string? entryId)
    {
        return new FolderMenuValidationIssue(FolderMenuValidationSeverity.Error, message, code, entryId);
    }
}
