using Foldora.Cli;
using Foldora.Core.DesktopIni;
using Foldora.Tests.Fixtures;

namespace Foldora.Tests.Cli;

public sealed class DesktopIniPolicyDiagnosticsRunnerTests
{
    [Fact]
    public async Task RunAsync_CreatesOneFolderPerSupportedPolicy()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDiagnostics-");
        var iconPath = Path.Combine(root.FullName, "icon.ico");

        await IcoTestFile.WriteValidAsync(iconPath);

        try
        {
            var output = new StringWriter();
            var runner = new DesktopIniPolicyDiagnosticsRunner();

            var createdFolders = await runner.RunAsync(root.FullName, iconPath, output);

            Assert.Equal(DesktopIniAttributePolicy.Supported.Count, createdFolders.Count);
            foreach (var policy in DesktopIniAttributePolicy.Supported)
            {
                var folderPath = Path.Combine(root.FullName, "Foldora Policy Test - " + policy.DiagnosticFolderName);
                Assert.True(Directory.Exists(folderPath));
                Assert.True(File.Exists(Path.Combine(folderPath, DesktopIniService.FileName)));
            }

            Assert.Contains("Manual checklist:", output.ToString());
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RunAsync_AppliesDesktopIniToEachPolicyFolder()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDiagnostics-");
        var iconPath = Path.Combine(root.FullName, "icon.ico");

        await IcoTestFile.WriteValidAsync(iconPath);

        try
        {
            var runner = new DesktopIniPolicyDiagnosticsRunner();

            var createdFolders = await runner.RunAsync(root.FullName, iconPath, TextWriter.Null);

            foreach (var folderPath in createdFolders)
            {
                var content = await File.ReadAllTextAsync(Path.Combine(folderPath, DesktopIniService.FileName));
                Assert.Contains("[.ShellClassInfo]", content);
                Assert.Contains($"IconResource={Path.GetFullPath(iconPath)},0", content);
            }
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RunAsync_RejectsMissingTarget()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDiagnostics-");
        var iconPath = Path.Combine(root.FullName, "icon.ico");

        await IcoTestFile.WriteValidAsync(iconPath);

        try
        {
            var runner = new DesktopIniPolicyDiagnosticsRunner();

            await Assert.ThrowsAsync<DirectoryNotFoundException>(
                () => runner.RunAsync(Path.Combine(root.FullName, "Missing"), iconPath, TextWriter.Null));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RunAsync_RejectsMissingIcon()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDiagnostics-");

        try
        {
            var runner = new DesktopIniPolicyDiagnosticsRunner();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => runner.RunAsync(root.FullName, Path.Combine(root.FullName, "missing.ico"), TextWriter.Null));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RunAsync_RejectsNonIcoIcon()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraDiagnostics-");
        var iconPath = Path.Combine(root.FullName, "icon.png");

        await File.WriteAllTextAsync(iconPath, "not ico");

        try
        {
            var runner = new DesktopIniPolicyDiagnosticsRunner();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => runner.RunAsync(root.FullName, iconPath, TextWriter.Null));
        }
        finally
        {
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
