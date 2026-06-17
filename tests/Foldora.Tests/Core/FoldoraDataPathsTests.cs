using Foldora.Core.Storage;

namespace Foldora.Tests.Core;

public sealed class FoldoraDataPathsTests
{
    [Fact]
    public void CreateDefault_ReturnsAppDataFoldoraRoot()
    {
        var paths = FoldoraDataPaths.CreateDefault();

        Assert.Contains("Foldora", paths.RootDirectory);
        Assert.StartsWith(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            paths.RootDirectory);
    }

    [Fact]
    public void Constructor_BuildsSettingsFileInsideRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "Foldora Path Test");

        var paths = new FoldoraDataPaths(root);

        Assert.Equal(Path.Combine(root, "settings.json"), paths.SettingsFile);
        Assert.Equal(Path.Combine(root, "packs"), paths.PacksDirectory);
        Assert.Equal(Path.Combine(root, "icons"), paths.IconsDirectory);
    }
}
