using Foldora.Core.Validation;

namespace Foldora.Tests.Validation;

public sealed class FolderNameValidatorTests
{
    [Fact]
    public void Validate_AcceptsCyrillicFolderName()
    {
        var result = FolderNameValidator.Validate("Новая папка");

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Bad<Name")]
    [InlineData("Bad>Name")]
    [InlineData("Bad:Name")]
    [InlineData("Bad\"Name")]
    [InlineData("Bad/Name")]
    [InlineData("Bad\\Name")]
    [InlineData("Bad|Name")]
    [InlineData("Bad?Name")]
    [InlineData("Bad*Name")]
    public void Validate_RejectsInvalidWindowsFilenameCharacters(string value)
    {
        var result = FolderNameValidator.Validate(value);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("NUL")]
    [InlineData("COM1")]
    [InlineData("LPT1")]
    [InlineData("con")]
    public void Validate_RejectsReservedDeviceNames(string value)
    {
        var result = FolderNameValidator.Validate(value);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Череп.")]
    [InlineData("Череп ")]
    public void Validate_RejectsTrailingDotOrSpace(string value)
    {
        var result = FolderNameValidator.Validate(value);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsTooLongName()
    {
        var result = FolderNameValidator.Validate(new string('a', 81));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsControlCharacters()
    {
        var result = FolderNameValidator.Validate("Bad\u0001Name");

        Assert.False(result.IsValid);
    }
}
