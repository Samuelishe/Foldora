namespace Foldora.MenuHost;

internal interface IDesktopTargetDetector
{
    bool IsDesktopDirectory(string targetDirectory);

    DesktopTargetDetectionResult Detect(string targetDirectory);
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
        return Detect(targetDirectory).IsDesktopTarget;
    }

    public DesktopTargetDetectionResult Detect(string targetDirectory)
    {
        if (string.IsNullOrWhiteSpace(targetDirectory))
        {
            return new DesktopTargetDetectionResult(
                targetDirectory,
                null,
                null,
                null,
                false,
                "Target directory is empty.");
        }

        var desktopPath = desktopPathProvider();
        if (string.IsNullOrWhiteSpace(desktopPath))
        {
            return new DesktopTargetDetectionResult(
                targetDirectory,
                null,
                desktopPath,
                null,
                false,
                "Desktop directory path could not be resolved.");
        }

        try
        {
            var normalizedTarget = NormalizeDirectoryPath(targetDirectory);
            var normalizedDesktop = NormalizeDirectoryPath(desktopPath);
            var isDesktopTarget = string.Equals(normalizedTarget, normalizedDesktop, StringComparison.OrdinalIgnoreCase);
            return new DesktopTargetDetectionResult(
                targetDirectory,
                normalizedTarget,
                desktopPath,
                normalizedDesktop,
                isDesktopTarget,
                isDesktopTarget ? null : "Target directory is not the current user Desktop directory.");
        }
        catch (Exception exception) when (exception is ArgumentException
                                         or NotSupportedException
                                         or PathTooLongException
                                         or System.Security.SecurityException)
        {
            return new DesktopTargetDetectionResult(
                targetDirectory,
                null,
                desktopPath,
                null,
                false,
                $"{exception.GetType().Name}: {exception.Message}");
        }
    }

    private static string NormalizeDirectoryPath(string path)
    {
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }
}
