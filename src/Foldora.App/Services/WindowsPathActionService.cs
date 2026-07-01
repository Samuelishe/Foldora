using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Foldora.App.Services;

/// <summary>
/// Windows implementation действий с путями для WPF SettingsWindow.
/// </summary>
public sealed class WindowsPathActionService : IPathActionService
{
    public void OpenFolder(string path)
    {
        var folder = ResolveFolder(path);
        Process.Start(new ProcessStartInfo
        {
            FileName = folder,
            UseShellExecute = true
        });
    }

    public void OpenLocation(string path)
    {
        if (File.Exists(path))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{path}\"",
                UseShellExecute = true
            });
            return;
        }

        OpenFolder(path);
    }

    public void CopyPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("Path is empty.");
        }

        Clipboard.SetText(path);
    }

    private static string ResolveFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("Path is empty.");
        }

        if (Directory.Exists(path))
        {
            return path;
        }

        if (File.Exists(path))
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            {
                return directory;
            }
        }

        throw new DirectoryNotFoundException($"Path was not found: {path}");
    }
}
