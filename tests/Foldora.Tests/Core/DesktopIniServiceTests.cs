using Foldora.Core.DesktopIni;

namespace Foldora.Tests.Core;

public sealed class DesktopIniServiceTests
{
    [Fact]
    public void CreateDesktopIniContent_ContainsShellClassInfoAndIconResource()
    {
        var iconPath = Path.Combine(Path.GetTempPath(), "Foldora Icons", "code.ico");

        var content = DesktopIniService.CreateDesktopIniContent(iconPath);

        Assert.Contains("[.ShellClassInfo]", content);
        Assert.Contains($"IconResource={Path.GetFullPath(iconPath)},0", content);
    }

    [Fact]
    public async Task ApplyIconAsync_WritesDesktopIniInsideTargetFolder()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraTests-");
        var folder = Directory.CreateDirectory(Path.Combine(root.FullName, "Папка с пробелами"));
        var iconPath = Path.Combine(root.FullName, "иконка test.ico");

        await File.WriteAllTextAsync(iconPath, "bootstrap placeholder");

        try
        {
            var service = new DesktopIniService();

            await service.ApplyIconAsync(new DesktopIniOptions(folder.FullName, iconPath));

            var desktopIniPath = Path.Combine(folder.FullName, DesktopIniService.FileName);
            var content = await File.ReadAllTextAsync(desktopIniPath);

            Assert.Contains("[.ShellClassInfo]", content);
            Assert.Contains($"IconResource={Path.GetFullPath(iconPath)},0", content);
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    private static void ClearAttributes(DirectoryInfo directory)
    {
        foreach (var file in directory.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            file.Attributes = FileAttributes.Normal;
        }

        foreach (var childDirectory in directory.EnumerateDirectories("*", SearchOption.AllDirectories))
        {
            childDirectory.Attributes = FileAttributes.Normal;
        }

        directory.Attributes = FileAttributes.Normal;
    }
}
