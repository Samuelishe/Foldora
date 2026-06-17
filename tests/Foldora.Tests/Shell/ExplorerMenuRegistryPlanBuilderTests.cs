using Foldora.Core.Menu;
using Foldora.Shell.RegistryPlan;

namespace Foldora.Tests.Shell;

public sealed class ExplorerMenuRegistryPlanBuilderTests
{
    [Fact]
    public void Build_CreatesDeleteOperationOnlyForOwnedDirectoryBackgroundRoot()
    {
        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(CreateEntry("entry-skull", "Череп")));

        var delete = Assert.Single(plan.DeleteOperations);
        Assert.Equal(ExplorerMenuRegistryHive.CurrentUser, delete.Hive);
        Assert.Equal(ExplorerMenuRegistryPaths.DirectoryBackgroundRoot, delete.KeyPath);
    }

    [Fact]
    public void Build_CreatesPlanForDirectoryBackground()
    {
        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(CreateEntry("entry-skull", "Череп")));

        Assert.Equal(ExplorerMenuTargetKind.DirectoryBackground, plan.TargetKind);
        Assert.All(plan.KeyOperations, operation => Assert.StartsWith(ExplorerMenuRegistryPaths.DirectoryBackgroundRoot, operation.KeyPath));
    }

    [Fact]
    public void Build_CreatesPlanForDirectory()
    {
        var plan = BuildPlan(ExplorerMenuTargetKind.Directory, CreateSettings(CreateEntry("entry-skull", "Череп")));

        Assert.Equal(ExplorerMenuTargetKind.Directory, plan.TargetKind);
        Assert.All(plan.KeyOperations, operation => Assert.StartsWith(ExplorerMenuRegistryPaths.DirectoryRoot, operation.KeyPath));
    }

    [Fact]
    public void Build_EntryCommandCallsCreateWithTargetAndEntryId()
    {
        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(CreateEntry("entry-skull", "Череп")));

        var command = Assert.Single(plan.ValueOperations, operation => operation.KeyPath.EndsWith(@"\command") && operation.ValueName == string.Empty);

        Assert.Contains("create", command.ValueData);
        Assert.Contains("--target", command.ValueData);
        Assert.Contains(@"""%V""", command.ValueData);
        Assert.Contains("--entry-id", command.ValueData);
        Assert.Contains(@"""entry-skull""", command.ValueData);
    }

    [Fact]
    public void Build_UsesDirectoryPlaceholderForDirectoryTarget()
    {
        var plan = BuildPlan(ExplorerMenuTargetKind.Directory, CreateSettings(CreateEntry("entry-skull", "Череп")));

        var command = Assert.Single(plan.ValueOperations, operation => operation.KeyPath.EndsWith(@"\command") && operation.ValueName == string.Empty);

        Assert.Contains(@"""%1""", command.ValueData);
    }

    [Fact]
    public void Build_DoesNotIncludeDisabledEntries()
    {
        var enabled = CreateEntry("entry-enabled", "Череп");
        var disabled = CreateEntry("entry-disabled", "Скелет");
        disabled.IsEnabled = false;

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(enabled, disabled));

        Assert.Contains(plan.ValueOperations, operation => operation.ValueData.Contains("entry-enabled"));
        Assert.DoesNotContain(plan.ValueOperations, operation => operation.ValueData.Contains("entry-disabled"));
    }

    [Fact]
    public void Build_AllowsDuplicateDisplayNamesAndIncludesBothEntries()
    {
        var plan = BuildPlan(
            ExplorerMenuTargetKind.DirectoryBackground,
            CreateSettings(
                CreateEntry("entry-skull-1", "Череп"),
                CreateEntry("entry-skull-2", "Череп")));

        var commands = plan.ValueOperations
            .Where(operation => operation.KeyPath.EndsWith(@"\command") && operation.ValueName == string.Empty)
            .ToArray();

        Assert.Equal(2, commands.Length);
        Assert.Contains(commands, operation => operation.ValueData.Contains("entry-skull-1"));
        Assert.Contains(commands, operation => operation.ValueData.Contains("entry-skull-2"));
    }

    [Fact]
    public void Build_DoesNotUseDisplayNameAsRegistryKeyName()
    {
        var displayName = "💀 Череп";

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(CreateEntry("entry-skull", displayName)));

        Assert.DoesNotContain(plan.KeyOperations, operation => operation.KeyPath.Contains(displayName, StringComparison.Ordinal));
        Assert.Contains(plan.ValueOperations, operation => operation.ValueName == "MUIVerb" && operation.ValueData == displayName);
    }

    [Fact]
    public void Build_QuotesCliPathWithSpacesAndCyrillic()
    {
        var cliPath = @"C:\Program Files\Фолдора\Foldora.Cli.exe";

        var plan = new ExplorerMenuRegistryPlanBuilder().Build(
            cliPath,
            CreateSettings(CreateEntry("entry-skull", "Череп")),
            ExplorerMenuTargetKind.DirectoryBackground);

        var command = Assert.Single(plan.ValueOperations, operation => operation.KeyPath.EndsWith(@"\command") && operation.ValueName == string.Empty);
        Assert.StartsWith(@"""C:\Program Files\Фолдора\Foldora.Cli.exe""", command.ValueData);
    }

    [Fact]
    public void Build_EmptyEnabledEntriesCreatesOnlyDeleteOperations()
    {
        var disabled = CreateEntry("entry-disabled", "Череп");
        disabled.IsEnabled = false;

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(disabled));

        Assert.Single(plan.DeleteOperations);
        Assert.Empty(plan.KeyOperations);
        Assert.Empty(plan.ValueOperations);
    }

    [Fact]
    public void Build_DoesNotContainOperationsOutsideOwnedRoots()
    {
        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(CreateEntry("entry-skull", "Череп")));
        var validator = new ExplorerMenuRegistryPlanValidator();

        var result = validator.Validate(plan);

        Assert.True(result.IsValid);
        Assert.All(plan.DeleteOperations, operation => Assert.True(ExplorerMenuRegistryPaths.IsInsideOwnedRoot(operation.KeyPath)));
        Assert.All(plan.KeyOperations, operation => Assert.True(ExplorerMenuRegistryPaths.IsInsideOwnedRoot(operation.KeyPath)));
        Assert.All(plan.ValueOperations, operation => Assert.True(ExplorerMenuRegistryPaths.IsInsideOwnedRoot(operation.KeyPath)));
    }

    private static ExplorerMenuRegistryPlan BuildPlan(ExplorerMenuTargetKind targetKind, FolderMenuSettings settings)
    {
        return new ExplorerMenuRegistryPlanBuilder().Build(
            @"C:\Program Files\Foldora\Foldora.Cli.exe",
            settings,
            targetKind);
    }

    private static FolderMenuSettings CreateSettings(params FolderMenuEntry[] entries)
    {
        var settings = new FolderMenuSettings();
        for (var index = 0; index < entries.Length; index++)
        {
            entries[index].SortOrder = index;
            settings.Entries.Add(entries[index]);
        }

        return settings;
    }

    private static FolderMenuEntry CreateEntry(string id, string displayName)
    {
        return new FolderMenuEntry
        {
            Id = id,
            DisplayName = displayName,
            DefaultFolderName = "Новая папка",
            IconPath = $@"C:\Foldora\icons\{id}.ico",
            IsEnabled = true
        };
    }
}
