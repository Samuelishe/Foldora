using System.IO;
using System.Text;
using Foldora.Core.Storage;

namespace Foldora.App.Services;

/// <summary>
/// Минимальная диагностика startup-ошибок без полноценного logging framework.
/// </summary>
public static class StartupDiagnosticsService
{
    public const string LogFileName = "startup-error.log";

    public static string WriteStartupError(Exception exception, string? rootDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var root = string.IsNullOrWhiteSpace(rootDirectory)
            ? FoldoraDataPaths.CreateDefault().RootDirectory
            : rootDirectory;
        var logsDirectory = Path.Combine(root, "Logs");
        Directory.CreateDirectory(logsDirectory);

        var logPath = Path.Combine(logsDirectory, LogFileName);
        var content = string.Join(
            Environment.NewLine,
            $"Timestamp: {DateTimeOffset.Now:O}",
            exception.ToString(),
            string.Empty);

        File.WriteAllText(logPath, content, Encoding.UTF8);
        return logPath;
    }
}
