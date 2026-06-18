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
    public void DefaultAttributePolicy_MatchesCurrentCompatibilityBehavior()
    {
        var policy = DesktopIniAttributePolicy.Default;

        Assert.Same(DesktopIniAttributePolicy.CompatibilitySystem, policy);
        Assert.Equal(FileAttributes.System, policy.FolderAttributes);
        Assert.Equal(FileAttributes.Hidden | FileAttributes.System, policy.DesktopIniAttributes);
    }

    [Theory]
    [MemberData(nameof(AttributePolicyCases))]
    public async Task ApplyIconAsync_AppliesSelectedAttributePolicy(
        DesktopIniAttributePolicy policy,
        FileAttributes expectedFolderAttributes,
        FileAttributes expectedDesktopIniAttributes)
    {
        var root = Directory.CreateTempSubdirectory("FoldoraTests-");
        var folder = Directory.CreateDirectory(Path.Combine(root.FullName, policy.Name));
        var iconPath = Path.Combine(root.FullName, "icon.ico");

        await File.WriteAllTextAsync(iconPath, "bootstrap placeholder");

        try
        {
            var service = new DesktopIniService();

            await service.ApplyIconAsync(new DesktopIniOptions(folder.FullName, iconPath, policy));

            var desktopIniPath = Path.Combine(folder.FullName, DesktopIniService.FileName);
            var desktopIniAttributes = File.GetAttributes(desktopIniPath);
            var folderAttributes = folder.Attributes;

            Assert.True(folderAttributes.HasFlag(expectedFolderAttributes));
            Assert.Equal(expectedDesktopIniAttributes, desktopIniAttributes & (FileAttributes.Hidden | FileAttributes.System));
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ApplyIconAsync_WithSelectedPolicy_KeepsDesktopIniContentShape()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraTests-");
        var folder = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));
        var iconPath = Path.Combine(root.FullName, "icon.ico");

        await File.WriteAllTextAsync(iconPath, "bootstrap placeholder");

        try
        {
            var service = new DesktopIniService();

            await service.ApplyIconAsync(new DesktopIniOptions(
                folder.FullName,
                iconPath,
                DesktopIniAttributePolicy.ReadOnlyFolderHiddenDesktopIni));

            var content = await File.ReadAllTextAsync(Path.Combine(folder.FullName, DesktopIniService.FileName));

            Assert.Contains("[.ShellClassInfo]", content);
            Assert.Contains($"IconResource={Path.GetFullPath(iconPath)},0", content);
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
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

    [Fact]
    public async Task ApplyIconAsync_RequiresExistingIconFile()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraTests-");
        var folder = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));
        var iconPath = Path.Combine(root.FullName, "missing.ico");

        try
        {
            var service = new DesktopIniService();

            await Assert.ThrowsAsync<FileNotFoundException>(
                () => service.ApplyIconAsync(new DesktopIniOptions(folder.FullName, iconPath)));
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ApplyIconAsync_RequiresIcoExtension()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraTests-");
        var folder = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));
        var iconPath = Path.Combine(root.FullName, "icon.png");

        await File.WriteAllTextAsync(iconPath, "not an ico");

        try
        {
            var service = new DesktopIniService();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ApplyIconAsync(new DesktopIniOptions(folder.FullName, iconPath)));
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ClearIconAsync_RemovesDesktopIniWhenOnlyFoldoraIconEntryExists()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraTests-");
        var folder = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));
        var iconPath = Path.Combine(root.FullName, "icon.ico");

        await File.WriteAllTextAsync(iconPath, "bootstrap placeholder");

        try
        {
            var service = new DesktopIniService();
            await service.ApplyIconAsync(new DesktopIniOptions(folder.FullName, iconPath));

            await service.ClearIconAsync(folder.FullName);

            Assert.False(File.Exists(Path.Combine(folder.FullName, DesktopIniService.FileName)));
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ClearIconAsync_PreservesOtherDesktopIniSections()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraTests-");
        var folder = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));
        var desktopIniPath = Path.Combine(folder.FullName, DesktopIniService.FileName);

        await File.WriteAllTextAsync(
            desktopIniPath,
            string.Join(
                Environment.NewLine,
                "[.ShellClassInfo]",
                "IconResource=C:\\Icons\\old.ico,0",
                "[OtherSection]",
                "Value=Keep",
                string.Empty));

        try
        {
            var service = new DesktopIniService();

            await service.ClearIconAsync(folder.FullName);

            var content = await File.ReadAllTextAsync(desktopIniPath);
            Assert.DoesNotContain("IconResource=", content);
            Assert.Contains("[OtherSection]", content);
            Assert.Contains("Value=Keep", content);
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ClearIconAsync_DoesNotThrowWhenDesktopIniDoesNotExist()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraTests-");
        var folder = Directory.CreateDirectory(Path.Combine(root.FullName, "Target"));

        try
        {
            var service = new DesktopIniService();

            await service.ClearIconAsync(folder.FullName);
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

    public static TheoryData<DesktopIniAttributePolicy, FileAttributes, FileAttributes> AttributePolicyCases()
    {
        return new TheoryData<DesktopIniAttributePolicy, FileAttributes, FileAttributes>
        {
            {
                DesktopIniAttributePolicy.CompatibilitySystem,
                FileAttributes.System,
                FileAttributes.Hidden | FileAttributes.System
            },
            {
                DesktopIniAttributePolicy.ReadOnlyFolderSystemDesktopIni,
                FileAttributes.ReadOnly,
                FileAttributes.Hidden | FileAttributes.System
            },
            {
                DesktopIniAttributePolicy.ReadOnlyFolderHiddenDesktopIni,
                FileAttributes.ReadOnly,
                FileAttributes.Hidden
            },
            {
                DesktopIniAttributePolicy.SystemFolderHiddenDesktopIni,
                FileAttributes.System,
                FileAttributes.Hidden
            }
        };
    }
}
