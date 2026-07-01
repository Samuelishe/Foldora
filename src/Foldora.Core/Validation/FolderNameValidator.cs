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
            issues.Add(Error(
                $"Default folder name must be {MaxLength} characters or shorter.",
                FolderMenuValidationIssueCodes.FolderNameTooLong,
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
                "Default folder name must not contain control characters.",
                FolderMenuValidationIssueCodes.FolderNameControlChars,
                new Dictionary<string, string>
                {
                    ["characterCode"] = $"U+{(int)controlCharacter.Value:X4}"
                }));
        }

        var invalidCharacter = value.FirstOrDefault(character => InvalidFileNameCharacters.Contains(character));
        if (invalidCharacter != default)
        {
            issues.Add(Error(
                "Default folder name contains characters that are not allowed in Windows folder names.",
                FolderMenuValidationIssueCodes.FolderNameInvalidChars,
                new Dictionary<string, string>
                {
                    ["character"] = invalidCharacter.ToString()
                }));
        }

        if (ReservedNames.Contains(value))
        {
            issues.Add(Error(
                "Default folder name uses a reserved Windows device name.",
                FolderMenuValidationIssueCodes.FolderNameReserved,
                new Dictionary<string, string>
                {
                    ["reservedName"] = value
                }));
        }

        if (value.EndsWith('.') || value.EndsWith(' '))
        {
            issues.Add(Error(
                "Default folder name must not end with a dot or a space.",
                FolderMenuValidationIssueCodes.FolderNameTrailingDotOrSpace,
                new Dictionary<string, string>
                {
                    ["ending"] = value[^1].ToString()
                }));
        }

        return issues.Count == 0 ? FolderMenuValidationResult.Success : new FolderMenuValidationResult(issues);
    }

    private static FolderMenuValidationIssue Error(
        string message,
        string code,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        return new FolderMenuValidationIssue(FolderMenuValidationSeverity.Error, message, code, parameters: parameters);
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
