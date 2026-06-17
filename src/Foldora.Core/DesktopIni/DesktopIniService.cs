using System.Text;

namespace Foldora.Core.DesktopIni;

/// <summary>
/// Применяет и очищает иконку папки через desktop.ini.
/// </summary>
public sealed class DesktopIniService
{
    public const string FileName = "desktop.ini";
    public const string ShellClassInfoSection = "[.ShellClassInfo]";

    public async Task ApplyIconAsync(DesktopIniOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.FolderPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.IconPath);

        var folder = new DirectoryInfo(options.FolderPath);
        if (!folder.Exists)
        {
            throw new DirectoryNotFoundException($"Target folder was not found: {folder.FullName}");
        }

        var icon = new FileInfo(options.IconPath);
        var desktopIniPath = Path.Combine(folder.FullName, FileName);
        var content = CreateDesktopIniContent(icon.FullName);

        await File.WriteAllTextAsync(desktopIniPath, content, Encoding.Unicode, cancellationToken);

        File.SetAttributes(desktopIniPath, FileAttributes.Hidden | FileAttributes.System);
        folder.Attributes |= FileAttributes.System;
    }

    public Task ClearIconAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);
        cancellationToken.ThrowIfCancellationRequested();

        var folder = new DirectoryInfo(folderPath);
        if (!folder.Exists)
        {
            throw new DirectoryNotFoundException($"Target folder was not found: {folder.FullName}");
        }

        var desktopIniPath = Path.Combine(folder.FullName, FileName);
        if (File.Exists(desktopIniPath))
        {
            File.SetAttributes(desktopIniPath, FileAttributes.Normal);
            File.Delete(desktopIniPath);
        }

        folder.Attributes &= ~FileAttributes.System;
        return Task.CompletedTask;
    }

    public static string CreateDesktopIniContent(string iconPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(iconPath);

        return string.Join(
            Environment.NewLine,
            ShellClassInfoSection,
            $"IconResource={Path.GetFullPath(iconPath)},0",
            string.Empty);
    }
}
