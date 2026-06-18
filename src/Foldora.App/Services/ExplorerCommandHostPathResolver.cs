using System.IO;

namespace Foldora.App.Services;

/// <summary>
/// Ищет no-console `Foldora.MenuHost.exe` для registry commands.
/// </summary>
public sealed class ExplorerCommandHostPathResolver : IExplorerCommandHostPathResolver
{
    public string ResolveCommandHostPath()
    {
        var processPath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Current process path could not be resolved.");

        if (IsMenuHostExecutable(processPath))
        {
            return processPath;
        }

        var directory = Path.GetDirectoryName(processPath)
            ?? throw new InvalidOperationException("Current process directory could not be resolved.");

        var installedHostPath = Path.Combine(directory, "Foldora.MenuHost.exe");
        if (File.Exists(installedHostPath))
        {
            return installedHostPath;
        }

        var devHostPath = TryResolveDevelopmentExecutablePath(directory, "Foldora.MenuHost", "net10.0-windows", "Foldora.MenuHost.exe");
        if (devHostPath is not null)
        {
            return devHostPath;
        }

        var legacyCliPath = Path.Combine(directory, "Foldora.Cli.exe");
        if (File.Exists(legacyCliPath))
        {
            return legacyCliPath;
        }

        var devCliPath = TryResolveDevelopmentExecutablePath(directory, "Foldora.Cli", "net10.0", "Foldora.Cli.exe");
        return devCliPath ?? installedHostPath;
    }

    private static string? TryResolveDevelopmentExecutablePath(
        string appDirectory,
        string projectName,
        string targetFramework,
        string executableName)
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
            projectName,
            "bin",
            configurationDirectory.Name,
            targetFramework,
            executableName));

        return File.Exists(candidate) ? candidate : null;
    }

    private static bool IsMenuHostExecutable(string path)
    {
        return string.Equals(Path.GetFileName(path), "Foldora.MenuHost.exe", StringComparison.OrdinalIgnoreCase);
    }
}
