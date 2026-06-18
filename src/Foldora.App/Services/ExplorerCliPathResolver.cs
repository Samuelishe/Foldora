using System.IO;

namespace Foldora.App.Services;

/// <summary>
/// Ищет `Foldora.Cli.exe` рядом с текущим executable или в соседнем dev output.
/// </summary>
public sealed class ExplorerCliPathResolver : IExplorerCliPathResolver
{
    public string ResolveCliPath()
    {
        var processPath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Current process path could not be resolved.");

        if (IsCliExecutable(processPath))
        {
            return processPath;
        }

        var directory = Path.GetDirectoryName(processPath)
            ?? throw new InvalidOperationException("Current process directory could not be resolved.");

        var installedCliPath = Path.Combine(directory, "Foldora.Cli.exe");
        if (File.Exists(installedCliPath))
        {
            return installedCliPath;
        }

        var devCliPath = TryResolveDevelopmentCliPath(directory);
        if (devCliPath is not null)
        {
            return devCliPath;
        }

        return installedCliPath;
    }

    private static string? TryResolveDevelopmentCliPath(string appDirectory)
    {
        var targetFrameworkDirectory = new DirectoryInfo(appDirectory);
        var configurationDirectory = targetFrameworkDirectory.Parent;
        if (configurationDirectory is null)
        {
            return null;
        }

        var candidate = Path.GetFullPath(Path.Combine(
            appDirectory,
            "..",
            "..",
            "..",
            "..",
            "Foldora.Cli",
            "bin",
            configurationDirectory.Name,
            "net10.0",
            "Foldora.Cli.exe"));

        return File.Exists(candidate) ? candidate : null;
    }

    private static bool IsCliExecutable(string path)
    {
        return string.Equals(Path.GetFileName(path), "Foldora.Cli.exe", StringComparison.OrdinalIgnoreCase);
    }
}
