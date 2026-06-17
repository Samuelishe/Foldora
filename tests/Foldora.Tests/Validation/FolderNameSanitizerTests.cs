using Foldora.Core.Validation;

namespace Foldora.Tests.Validation;

public sealed class FolderNameSanitizerTests
{
    [Fact]
    public void Sanitize_ReplacesInvalidCharactersAndCollapsesSpaces()
    {
        var value = FolderNameSanitizer.Sanitize("  Bad<Name\t\tTest  ");

        Assert.Equal("Bad Name Test", value);
    }

    [Fact]
    public void Sanitize_ReturnsDefaultForEmptyResult()
    {
        var value = FolderNameSanitizer.Sanitize("\t\r\n");

        Assert.Equal("Новая папка", value);
    }

    [Fact]
    public void Sanitize_LimitsLength()
    {
        var value = FolderNameSanitizer.Sanitize(new string('a', 100));

        Assert.Equal(80, value.Length);
    }
}
