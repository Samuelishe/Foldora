namespace Foldora.Core.Validation;

/// <summary>
/// Проверяет имя папки на совместимость с правилами Windows filename.
/// </summary>
public static class FolderNameValidator
{
    public const string DefaultFolderName = "Новая папка";
    public const int MaxLength = 80;

    private static readonly char[] InvalidFileNameCharacters = ['<', '>', ':', '"', '/', '\\', '|', '?', '*'];

    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    public static string NormalizeOrDefault(string? folderName)
    {
        return string.IsNullOrWhiteSpace(folderName) ? DefaultFolderName : folderName;
    }

    public static FolderMenuValidationResult Validate(string? folderName)
    {
        var value = NormalizeOrDefault(folderName);
        var issues = new List<FolderMenuValidationIssue>();

        if (value.Length > MaxLength)
        {
            issues.Add(Error($"Default folder name must be {MaxLength} characters or shorter.", "folder_name_too_long"));
        }

        if (value.Any(char.IsControl))
        {
            issues.Add(Error("Default folder name must not contain control characters.", "folder_name_control_chars"));
        }

        if (value.IndexOfAny(InvalidFileNameCharacters) >= 0)
        {
            issues.Add(Error("Default folder name contains characters that are not allowed in Windows folder names.", "folder_name_invalid_chars"));
        }

        if (ReservedNames.Contains(value))
        {
            issues.Add(Error("Default folder name uses a reserved Windows device name.", "folder_name_reserved"));
        }

        if (value.EndsWith('.') || value.EndsWith(' '))
        {
            issues.Add(Error("Default folder name must not end with a dot or a space.", "folder_name_trailing_dot_or_space"));
        }

        return issues.Count == 0 ? FolderMenuValidationResult.Success : new FolderMenuValidationResult(issues);
    }

    private static FolderMenuValidationIssue Error(string message, string code)
    {
        return new FolderMenuValidationIssue(FolderMenuValidationSeverity.Error, message, code);
    }
}
