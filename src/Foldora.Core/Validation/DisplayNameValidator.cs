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
            issues.Add(Error("Display name must not be empty after fallback resolution.", "display_name_empty", entryId));
        }

        if (value.Length > MaxLength)
        {
            issues.Add(Error($"Display name must be {MaxLength} characters or shorter.", "display_name_too_long", entryId));
        }

        if (value.Any(char.IsControl))
        {
            issues.Add(Error("Display name must not contain control characters.", "display_name_control_chars", entryId));
        }

        return issues.Count == 0 ? FolderMenuValidationResult.Success : new FolderMenuValidationResult(issues);
    }

    private static FolderMenuValidationIssue Error(string message, string code, string? entryId)
    {
        return new FolderMenuValidationIssue(FolderMenuValidationSeverity.Error, message, code, entryId);
    }
}
