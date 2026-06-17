using System.Text;

namespace Foldora.Core.Validation;

/// <summary>
/// Удобная очистка пользовательского ввода имени папки для будущего WPF UI.
/// </summary>
public static class FolderNameSanitizer
{
    private static readonly HashSet<char> InvalidCharacters = ['<', '>', ':', '"', '/', '\\', '|', '?', '*'];

    public static string Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return FolderNameValidator.DefaultFolderName;
        }

        var builder = new StringBuilder(value.Length);
        var previousWasSpace = false;

        foreach (var character in value)
        {
            var nextCharacter = char.IsControl(character) || InvalidCharacters.Contains(character)
                ? ' '
                : character;

            if (char.IsWhiteSpace(nextCharacter))
            {
                if (!previousWasSpace)
                {
                    builder.Append(' ');
                    previousWasSpace = true;
                }

                continue;
            }

            builder.Append(nextCharacter);
            previousWasSpace = false;
        }

        var sanitized = builder.ToString().Trim();
        if (sanitized.Length > FolderNameValidator.MaxLength)
        {
            sanitized = sanitized[..FolderNameValidator.MaxLength].Trim();
        }

        return string.IsNullOrWhiteSpace(sanitized)
            ? FolderNameValidator.DefaultFolderName
            : sanitized;
    }
}
