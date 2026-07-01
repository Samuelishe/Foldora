using System.IO;

namespace Foldora.App.Services;

/// <summary>
/// Ищет no-console `Foldora.MenuHost.exe` для registry commands.
/// </summary>
public sealed class ExplorerCommandHostPathResolver : IExplorerCommandHostPathResolver
{
    private readonly Func<string?> processPathProvider;

    public ExplorerCommandHostPathResolver()
        : this(() => Environment.ProcessPath)
    {
    }

    public ExplorerCommandHostPathResolver(Func<string?> processPathProvider)
    {
        this.processPathProvider = processPathProvider ?? throw new ArgumentNullException(nameof(processPathProvider));
    }

    public string ResolveCommandHostPath()
    {
        var processPath = processPathProvider()
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

        throw new FileNotFoundException(
            "Foldora.MenuHost.exe was not found next to the current executable or in the development build output. Publish the dev layout or pass an explicit host path before enabling Explorer integration.",
            installedHostPath);
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
