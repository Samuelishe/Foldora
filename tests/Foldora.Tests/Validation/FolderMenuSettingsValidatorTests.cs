using Foldora.Core.Menu;
using Foldora.Core.Validation;

namespace Foldora.Tests.Validation;

public sealed class FolderMenuSettingsValidatorTests
{
    [Fact]
    public void Validate_AllowsFiftyEnabledEntries()
    {
        var settings = CreateSettings(50, enabled: true);

        var result = new FolderMenuSettingsValidator().Validate(settings);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsFiftyOneEnabledEntries()
    {
        var settings = CreateSettings(51, enabled: true);

        var result = new FolderMenuSettingsValidator().Validate(settings);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_AllowsDuplicateDisplayNames()
    {
        var settings = new FolderMenuSettings();
        settings.Entries.Add(CreateEntry(1, enabled: true, displayName: "Череп"));
        settings.Entries.Add(CreateEntry(2, enabled: true, displayName: "Череп"));

        var result = new FolderMenuSettingsValidator().Validate(settings);

        Assert.True(result.IsValid);
    }

    private static FolderMenuSettings CreateSettings(int count, bool enabled)
    {
        var settings = new FolderMenuSettings();
        for (var index = 0; index < count; index++)
        {
            settings.Entries.Add(CreateEntry(index, enabled, $"Вид {index}"));
        }

        return settings;
    }

    private static FolderMenuEntry CreateEntry(int index, bool enabled, string displayName)
    {
        return new FolderMenuEntry
        {
            Id = $"entry-{index}",
            DisplayName = displayName,
            DefaultFolderName = "Новая папка",
            IconPath = $"entry-{index}.ico",
            IsEnabled = enabled
        };
    }
}
