using Foldora.Core.Menu;

namespace Foldora.Tests.Menu;

public sealed class UniqueFolderNameServiceTests
{
    [Fact]
    public void GetAvailableDirectoryPath_ReturnsBaseNameWhenFree()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraUnique-");

        try
        {
            var path = new UniqueFolderNameService().GetAvailableDirectoryPath(root.FullName, "Череп");

            Assert.Equal(Path.Combine(root.FullName, "Череп"), path);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void GetAvailableDirectoryPath_ReturnsSecondNameWhenBaseExists()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraUnique-");

        try
        {
            Directory.CreateDirectory(Path.Combine(root.FullName, "Череп"));

            var path = new UniqueFolderNameService().GetAvailableDirectoryPath(root.FullName, "Череп");

            Assert.Equal(Path.Combine(root.FullName, "Череп (2)"), path);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void GetAvailableDirectoryPath_ReturnsThirdNameWhenFirstTwoExist()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraUnique-");

        try
        {
            Directory.CreateDirectory(Path.Combine(root.FullName, "Череп"));
            Directory.CreateDirectory(Path.Combine(root.FullName, "Череп (2)"));

            var path = new UniqueFolderNameService().GetAvailableDirectoryPath(root.FullName, "Череп");

            Assert.Equal(Path.Combine(root.FullName, "Череп (3)"), path);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task GetAvailableDirectoryPath_ConsidersFileConflicts()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraUnique-");

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root.FullName, "Череп"), "file conflict");

            var path = new UniqueFolderNameService().GetAvailableDirectoryPath(root.FullName, "Череп");

            Assert.Equal(Path.Combine(root.FullName, "Череп (2)"), path);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void GetAvailableDirectoryPath_ThrowsAfterMaxAttempts()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraUnique-");

        try
        {
            Directory.CreateDirectory(Path.Combine(root.FullName, "Череп"));
            Directory.CreateDirectory(Path.Combine(root.FullName, "Череп (2)"));

            Assert.Throws<InvalidOperationException>(
                () => new UniqueFolderNameService().GetAvailableDirectoryPath(root.FullName, "Череп", maxAttempts: 2));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
