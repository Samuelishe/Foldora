using System.Text;

namespace Foldora.Core.DesktopIni;

/// <summary>
/// Применяет и очищает иконку папки через desktop.ini.
/// </summary>
public sealed class DesktopIniService
{
    public const string FileName = "desktop.ini";
    public const string ShellClassInfoSection = "[.ShellClassInfo]";
    private const string IconResourceKey = "IconResource";

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
        if (!icon.Exists)
        {
            throw new FileNotFoundException($"Icon file was not found: {icon.FullName}", icon.FullName);
        }

        if (!string.Equals(icon.Extension, ".ico", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Foldora MVP supports only .ico files for folder icons.");
        }

        var desktopIniPath = Path.Combine(folder.FullName, FileName);
        var lines = File.Exists(desktopIniPath)
            ? await File.ReadAllLinesAsync(desktopIniPath, cancellationToken)
            : [];

        lines = UpsertIconResource(lines, icon.FullName);

        if (File.Exists(desktopIniPath))
        {
            File.SetAttributes(desktopIniPath, FileAttributes.Normal);
        }

        await File.WriteAllTextAsync(desktopIniPath, BuildContent(lines), Encoding.Unicode, cancellationToken);
        File.SetAttributes(desktopIniPath, FileAttributes.Hidden | FileAttributes.System);
        folder.Attributes |= FileAttributes.System;
    }

    public async Task ClearIconAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);
        cancellationToken.ThrowIfCancellationRequested();

        var folder = new DirectoryInfo(folderPath);
        if (!folder.Exists)
        {
            throw new DirectoryNotFoundException($"Target folder was not found: {folder.FullName}");
        }

        var desktopIniPath = Path.Combine(folder.FullName, FileName);
        if (!File.Exists(desktopIniPath))
        {
            return;
        }

        var lines = await File.ReadAllLinesAsync(desktopIniPath, cancellationToken);
        var cleanedLines = RemoveIconResource(lines);

        File.SetAttributes(desktopIniPath, FileAttributes.Normal);

        if (ContainsUsefulLines(cleanedLines))
        {
            await File.WriteAllTextAsync(desktopIniPath, BuildContent(cleanedLines), Encoding.Unicode, cancellationToken);
            File.SetAttributes(desktopIniPath, FileAttributes.Hidden | FileAttributes.System);
            return;
        }

        File.Delete(desktopIniPath);
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

    private static string BuildContent(IReadOnlyCollection<string> lines)
    {
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string[] UpsertIconResource(IReadOnlyList<string> lines, string iconPath)
    {
        var result = RemoveIconResource(lines).ToList();
        var iconLine = $"{IconResourceKey}={Path.GetFullPath(iconPath)},0";
        var sectionIndex = FindShellClassInfoSection(result);

        if (sectionIndex < 0)
        {
            if (result.Count > 0 && !string.IsNullOrWhiteSpace(result[^1]))
            {
                result.Add(string.Empty);
            }

            result.Add(ShellClassInfoSection);
            result.Add(iconLine);
            return result.ToArray();
        }

        var insertIndex = sectionIndex + 1;
        while (insertIndex < result.Count && !IsSectionHeader(result[insertIndex]))
        {
            insertIndex++;
        }

        result.Insert(insertIndex, iconLine);
        return result.ToArray();
    }

    private static string[] RemoveIconResource(IReadOnlyList<string> lines)
    {
        var result = new List<string>();
        var insideShellClassInfo = false;

        foreach (var line in lines)
        {
            if (IsSectionHeader(line))
            {
                insideShellClassInfo = IsShellClassInfoSection(line);
                result.Add(line);
                continue;
            }

            if (insideShellClassInfo && IsIconResourceLine(line))
            {
                continue;
            }

            result.Add(line);
        }

        return RemoveEmptyShellClassInfoSections(result);
    }

    private static string[] RemoveEmptyShellClassInfoSections(IReadOnlyList<string> lines)
    {
        var result = new List<string>();
        var index = 0;

        while (index < lines.Count)
        {
            if (!IsShellClassInfoSection(lines[index]))
            {
                result.Add(lines[index]);
                index++;
                continue;
            }

            var sectionStart = index;
            index++;
            var sectionLines = new List<string>();

            while (index < lines.Count && !IsSectionHeader(lines[index]))
            {
                sectionLines.Add(lines[index]);
                index++;
            }

            if (sectionLines.Any(line => !string.IsNullOrWhiteSpace(line)))
            {
                result.Add(lines[sectionStart]);
                result.AddRange(sectionLines);
            }
        }

        return result.ToArray();
    }

    private static int FindShellClassInfoSection(IReadOnlyList<string> lines)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            if (IsShellClassInfoSection(lines[index]))
            {
                return index;
            }
        }

        return -1;
    }

    private static bool ContainsUsefulLines(IEnumerable<string> lines)
    {
        return lines.Any(line => !string.IsNullOrWhiteSpace(line));
    }

    private static bool IsIconResourceLine(string line)
    {
        var separatorIndex = line.IndexOf('=');
        if (separatorIndex < 0)
        {
            return false;
        }

        return string.Equals(line[..separatorIndex].Trim(), IconResourceKey, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsShellClassInfoSection(string line)
    {
        return string.Equals(line.Trim(), ShellClassInfoSection, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSectionHeader(string line)
    {
        var trimmedLine = line.Trim();
        return trimmedLine.StartsWith('[') && trimmedLine.EndsWith(']');
    }
}
