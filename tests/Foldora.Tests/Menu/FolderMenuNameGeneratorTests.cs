using Foldora.Core.Menu;

namespace Foldora.Tests.Menu;

public sealed class FolderMenuNameGeneratorTests
{
    [Fact]
    public void GetNextName_ReturnsFirstNameForEmptyList()
    {
        var name = FolderMenuNameGenerator.GetNextName([]);

        Assert.Equal("Вид 1", name);
    }

    [Fact]
    public void GetNextName_ReturnsSecondNameWhenFirstIsTaken()
    {
        var name = FolderMenuNameGenerator.GetNextName(
            [new FolderMenuEntry { DisplayName = "Вид 1" }]);

        Assert.Equal("Вид 2", name);
    }

    [Fact]
    public void GetNextName_ReturnsFirstFreeNumber()
    {
        var name = FolderMenuNameGenerator.GetNextName(
            [
                new FolderMenuEntry { DisplayName = "Вид 1" },
                new FolderMenuEntry { DisplayName = "Вид 3" },
                new FolderMenuEntry { DisplayName = "Череп" }
            ]);

        Assert.Equal("Вид 2", name);
    }
}
