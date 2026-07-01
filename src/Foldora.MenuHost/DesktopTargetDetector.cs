namespace Foldora.MenuHost;

internal interface IDesktopTargetDetector
{
    bool IsDesktopDirectory(string targetDirectory);
}

internal sealed class DesktopTargetDetector : IDesktopTargetDetector
{
    private readonly Func<string> desktopPathProvider;

    public DesktopTargetDetector()
        : this(() => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))
    {
    }

    public DesktopTargetDetector(Func<string> desktopPathProvider)
    {
        ArgumentNullException.ThrowIfNull(desktopPathProvider);
        this.desktopPathProvider = desktopPathProvider;
    }

    public bool IsDesktopDirectory(string targetDirectory)
    {
        if (string.IsNullOrWhiteSpace(targetDirectory))
        {
            return false;
        }

        var desktopPath = desktopPathProvider();
        if (string.IsNullOrWhiteSpace(desktopPath))
        {
            return false;
        }

        try
        {
            var normalizedTarget = NormalizeDirectoryPath(targetDirectory);
            var normalizedDesktop = NormalizeDirectoryPath(desktopPath);
            return string.Equals(normalizedTarget, normalizedDesktop, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception exception) when (exception is ArgumentException
                                         or NotSupportedException
                                         or PathTooLongException
                                         or System.Security.SecurityException)
        {
            return false;
        }
    }

    private static string NormalizeDirectoryPath(string path)
    {
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }
}
