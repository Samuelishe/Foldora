using Foldora.Core.Menu;
using Foldora.Core.Storage;
using Foldora.Tests.Fixtures;

namespace Foldora.Tests.Menu;

public sealed class IconImportServiceTests
{
    [Fact]
    public async Task ImportAsync_CopiesIcoIntoAppDataIconsDirectory()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraImport-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var sourceIconPath = Path.Combine(root.FullName, "Downloads", "череп.ico");
            await IcoTestFile.WriteValidAsync(sourceIconPath);

            var result = await new IconImportService().ImportAsync(sourceIconPath, paths);

            Assert.True(File.Exists(result.ImportedIconPath));
            Assert.StartsWith(Path.GetFullPath(paths.IconsDirectory), Path.GetFullPath(result.ImportedIconPath));
            Assert.NotEqual(Path.GetFullPath(sourceIconPath), Path.GetFullPath(result.ImportedIconPath));
            Assert.EndsWith($"{result.EntryId}.ico", result.ImportedIconPath);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ImportAsync_RejectsMissingIcon()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraImport-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => new IconImportService().ImportAsync(Path.Combine(root.FullName, "missing.ico"), paths));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ImportAsync_RejectsNonIcoFile()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraImport-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var sourceIconPath = Path.Combine(root.FullName, "icon.png");
            await File.WriteAllTextAsync(sourceIconPath, "png placeholder");

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => new IconImportService().ImportAsync(sourceIconPath, paths));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
