namespace Foldora.Core.Validation;

/// <summary>
/// Проверяет подпись пункта меню.
/// </summary>
public static class DisplayNameValidator
{
    public const int MaxLength = 80;

    public static string Normalize(string displayName)
    {
        ArgumentNullException.ThrowIfNull(displayName);
        return displayName.Trim();
    }

    public static FolderMenuValidationResult ValidateResolved(string displayName, string? entryId = null)
    {
        var value = Normalize(displayName);
        var issues = new List<FolderMenuValidationIssue>();

        if (value.Length == 0)
        {
            issues.Add(Error("Display name must not be empty after fallback resolution.", FolderMenuValidationIssueCodes.DisplayNameEmpty, entryId));
        }

        if (value.Length > MaxLength)
        {
            issues.Add(Error(
                $"Display name must be {MaxLength} characters or shorter.",
                FolderMenuValidationIssueCodes.DisplayNameTooLong,
                entryId,
                new Dictionary<string, string>
                {
                    ["maxLength"] = MaxLength.ToString(),
                    ["actualLength"] = value.Length.ToString()
                }));
        }

        var controlCharacter = FindControlCharacter(value);
        if (controlCharacter.HasValue)
        {
            issues.Add(Error(
                "Display name must not contain control characters.",
                FolderMenuValidationIssueCodes.DisplayNameControlChars,
                entryId,
                new Dictionary<string, string>
                {
                    ["characterCode"] = $"U+{(int)controlCharacter.Value:X4}"
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
