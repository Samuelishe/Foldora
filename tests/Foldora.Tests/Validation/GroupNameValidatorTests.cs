using Foldora.Core.Validation;

namespace Foldora.Tests.Validation;

public sealed class GroupNameValidatorTests
{
    [Fact]
    public void Validate_AllowsCyrillicGroupName()
    {
        var result = GroupNameValidator.Validate("Цветные");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_AllowsEmptyGroupName()
    {
        var result = GroupNameValidator.Validate("   ");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsTooLongGroupName()
    {
        var result = GroupNameValidator.Validate(new string('А', 81));

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "group_name_too_long");
    }

    [Fact]
    public void Validate_RejectsControlCharacters()
    {
        var result = GroupNameValidator.Validate("Bad\u0001Group");

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "group_name_control_chars");
    }

    [Theory]
    [InlineData("Цветные/Тёмные")]
    [InlineData(@"Цветные\Тёмные")]
    public void Validate_RejectsNestedGroupSeparators(string value)
    {
        var result = GroupNameValidator.Validate(value);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "group_name_nested_not_supported");
    }
}
