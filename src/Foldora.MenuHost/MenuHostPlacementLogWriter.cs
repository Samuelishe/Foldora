using System.Text.Json;
using Foldora.Core.Storage;

namespace Foldora.MenuHost;

internal interface IMenuHostPlacementLogWriter
{
    void Append(MenuHostPlacementLogEntry entry);
}

internal sealed class MenuHostPlacementLogWriter : IMenuHostPlacementLogWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly string logPath;

    public MenuHostPlacementLogWriter()
        : this(Path.Combine(FoldoraDataPaths.CreateDefault().RootDirectory, "Logs", "menuhost-placement.log"))
    {
    }

    public MenuHostPlacementLogWriter(string logPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logPath);
        this.logPath = logPath;
    }

    public void Append(MenuHostPlacementLogEntry entry)
    {
        try
        {
            var directory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(entry, JsonOptions);
            File.AppendAllText(logPath, json + Environment.NewLine);
        }
        catch
        {
            // MenuHost must never fail Explorer commands because diagnostics cannot be written.
        }
    }
}
