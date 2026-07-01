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
            issues.Add(Error(
                $"Group name must be at most {MaxLength} characters.",
                FolderMenuValidationIssueCodes.GroupNameTooLong,
                entryId,
                new Dictionary<string, string>
                {
                    ["maxLength"] = MaxLength.ToString(),
                    ["actualLength"] = normalized.Length.ToString()
                }));
        }

        var controlCharacter = FindControlCharacter(normalized);
        if (controlCharacter.HasValue)
        {
            issues.Add(Error(
                "Group name must not contain control characters.",
                FolderMenuValidationIssueCodes.GroupNameControlChars,
                entryId,
                new Dictionary<string, string>
                {
                    ["characterCode"] = $"U+{(int)controlCharacter.Value:X4}"
                }));
        }

        var separator = normalized.Contains('/') ? "/" : normalized.Contains('\\') ? "\\" : string.Empty;
        if (separator.Length > 0)
        {
            issues.Add(Error(
                "Nested groups are not supported yet. Remove '/' and '\\' from the group name.",
                FolderMenuValidationIssueCodes.GroupNameNestedNotSupported,
                entryId,
                new Dictionary<string, string>
                {
                    ["separator"] = separator
                }));
        }

        return issues.Count == 0 ? FolderMenuValidationResult.Success : new FolderMenuValidationResult(issues);
    }

    private static FolderMenuValidationIssue Error(
        string message,
        string code,
        string? entryId,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        return new FolderMenuValidationIssue(FolderMenuValidationSeverity.Error, message, code, entryId, parameters);
    }

    private static char? FindControlCharacter(string value)
    {
        foreach (var character in value)
        {
            if (char.IsControl(character))
            {
                return character;
            }
        }

        return null;
    }
}
