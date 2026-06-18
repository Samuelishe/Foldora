using Foldora.Core.Menu;
using Foldora.Core.Validation;

namespace Foldora.Tests.Menu;

public sealed class FolderMenuEntryTests
{
    [Fact]
    public void Constructor_UsesDefaultFolderName()
    {
        var entry = new FolderMenuEntry();

        Assert.Equal(FolderNameValidator.DefaultFolderName, entry.DefaultFolderName);
    }

    [Fact]
    public void Constructor_UsesRootLevelGroupByDefault()
    {
        var entry = new FolderMenuEntry();

        Assert.Equal(string.Empty, entry.GroupName);
    }
}
