using Foldora.Core.Menu;
using Foldora.Core.Validation;

namespace Foldora.Tests.Validation;

public sealed class DisplayNameValidatorTests
{
    [Theory]
    [InlineData("Череп")]
    [InlineData("💀 Череп")]
    public void ValidateResolved_AllowsCyrillicAndEmoji(string value)
    {
        var result = DisplayNameValidator.ValidateResolved(value);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateResolved_RejectsControlCharacters()
    {
        var result = DisplayNameValidator.ValidateResolved("Bad\u0001Name");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateResolved_RejectsTooLongText()
    {
        var result = DisplayNameValidator.ValidateResolved(new string('a', 81));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void MenuSettingsValidator_AllowsDuplicateDisplayNames()
    {
        var settings = new FolderMenuSettings();
        settings.Entries.Add(CreateEntry("entry-1", "Череп"));
        settings.Entries.Add(CreateEntry("entry-2", "Череп"));

        var result = new FolderMenuSettingsValidator().Validate(settings);

        Assert.True(result.IsValid);
    }

    private static FolderMenuEntry CreateEntry(string id, string displayName)
    {
        return new FolderMenuEntry
        {
            Id = id,
            DisplayName = displayName,
            IconPath = $"{id}.ico"
        };
    }
}
