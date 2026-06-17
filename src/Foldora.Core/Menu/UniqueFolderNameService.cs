using Foldora.Core.Validation;

namespace Foldora.Core.Menu;

/// <summary>
/// Подбирает свободное имя папки без перезаписи существующих файлов и каталогов.
/// </summary>
public sealed class UniqueFolderNameService
{
    public const int DefaultMaxAttempts = 1000;

    public string GetAvailableDirectoryPath(
        string targetDirectory,
        string? baseName,
        int maxAttempts = DefaultMaxAttempts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetDirectory);

        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be at least 1.");
        }

        var target = new DirectoryInfo(targetDirectory);
        if (!target.Exists)
        {
            throw new DirectoryNotFoundException($"Target directory was not found: {target.FullName}");
        }

        var resolvedBaseName = FolderNameValidator.NormalizeOrDefault(baseName);
        var validation = FolderNameValidator.Validate(resolvedBaseName);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.Issues.First().Message);
        }

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var candidateName = attempt == 1
                ? resolvedBaseName
                : $"{resolvedBaseName} ({attempt})";
            var candidatePath = Path.Combine(target.FullName, candidateName);

            if (!Directory.Exists(candidatePath) && !File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        throw new InvalidOperationException($"Could not find an available folder name after {maxAttempts} attempts.");
    }
}
