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

    [Fact]
    public void Validate_AllowsDuplicateGroupNames()
    {
        var settings = new FolderMenuSettings();
        settings.Entries.Add(CreateEntry(1, enabled: true, displayName: "Синяя", groupName: "Цветные"));
        settings.Entries.Add(CreateEntry(2, enabled: true, displayName: "Красная", groupName: "Цветные"));

        var result = new FolderMenuSettingsValidator().Validate(settings);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsMoreThanThirtyGroups()
    {
        var settings = new FolderMenuSettings();
        for (var index = 0; index < 31; index++)
        {
            settings.Entries.Add(CreateEntry(index, enabled: true, $"Вид {index}", $"Группа {index}"));
        }

        var result = new FolderMenuSettingsValidator().Validate(settings);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "menu_group_limit");
    }

    [Fact]
    public void Validate_RejectsMoreThanThirtyChildrenPerGroup()
    {
        var settings = new FolderMenuSettings();
        for (var index = 0; index < 31; index++)
        {
            settings.Entries.Add(CreateEntry(index, enabled: true, $"Вид {index}", "Цветные"));
        }

        var result = new FolderMenuSettingsValidator().Validate(settings);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "menu_group_children_limit");
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

    private static FolderMenuEntry CreateEntry(int index, bool enabled, string displayName, string groupName = "")
    {
        return new FolderMenuEntry
        {
            Id = $"entry-{index}",
            DisplayName = displayName,
            DefaultFolderName = "Новая папка",
            GroupName = groupName,
            IconPath = $"entry-{index}.ico",
            IsEnabled = enabled
        };
    }
}
