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
        Assert.DoesNotContain(plan.KeyOperations, operation => operation.KeyPath.Contains(@"\create-folder", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Build_CreatesPlanForDirectory()
    {
        var plan = BuildPlan(ExplorerMenuTargetKind.Directory, CreateSettings(CreateEntry("entry-skull", "Череп")));

        Assert.Equal(ExplorerMenuTargetKind.Directory, plan.TargetKind);
        Assert.All(plan.KeyOperations, operation => Assert.StartsWith(ExplorerMenuRegistryPaths.DirectoryRoot, operation.KeyPath));
        Assert.DoesNotContain(plan.KeyOperations, operation => operation.KeyPath.Contains(@"\create-folder", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Build_UsesCreateFolderMenuTitleAsRootMuiVerb()
    {
        var settings = CreateSettings(CreateEntry("entry-skull", "Череп"));
        settings = new FolderMenuSettings { Title = "Мои папки", Entries = settings.Entries };

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, settings);

        Assert.Contains(
            plan.ValueOperations,
            operation => operation.KeyPath == ExplorerMenuRegistryPaths.DirectoryBackgroundRoot
                         && operation.ValueName == "MUIVerb"
                         && operation.ValueData == "Мои папки");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Build_UsesFallbackTitleWhenCreateFolderMenuTitleIsEmpty(string title)
    {
        var settings = CreateSettings(CreateEntry("entry-skull", "Череп"));
        settings = new FolderMenuSettings { Title = title, Entries = settings.Entries };

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, settings);

        Assert.Contains(
            plan.ValueOperations,
            operation => operation.KeyPath == ExplorerMenuRegistryPaths.DirectoryBackgroundRoot
                         && operation.ValueName == "MUIVerb"
                         && operation.ValueData == "Создать папку");
    }

    [Fact]
    public void Build_PutsEntriesDirectlyUnderOwnedRootShell()
    {
        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(CreateEntry("entry-skull", "Череп")));

        Assert.Contains(
            plan.KeyOperations,
            operation => operation.KeyPath == $@"{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}\shell\entry-001-entry-skull");
        Assert.Contains(
            plan.KeyOperations,
            operation => operation.KeyPath == $@"{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}\shell\entry-001-entry-skull\command");
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
    public void Build_WritesEntryIconValueWhenIconFileExists()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraPlanIcon-");

        try
        {
            var iconPath = Path.Combine(root.FullName, "entry-skull.ico");
            File.WriteAllText(iconPath, "fake icon");
            var entry = CreateEntry("entry-skull", "Череп");
            entry.IconPath = iconPath;

            var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(entry));

            Assert.Contains(
                plan.ValueOperations,
                operation => operation.KeyPath == $@"{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}\shell\entry-001-entry-skull"
                             && operation.ValueName == "Icon"
                             && operation.ValueData == iconPath);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void Build_DoesNotWriteIconValueWhenIconPathIsEmpty()
    {
        var entry = CreateEntry("entry-skull", "Череп");
        entry.IconPath = string.Empty;

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(entry));

        Assert.DoesNotContain(plan.ValueOperations, operation => operation.ValueName == "Icon");
    }

    [Fact]
    public void Build_DoesNotWriteIconValueWhenIconFileIsMissing()
    {
        var entry = CreateEntry("entry-skull", "Череп");
        entry.IconPath = @"C:\Foldora\icons\missing.ico";

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(entry));

        Assert.DoesNotContain(plan.ValueOperations, operation => operation.ValueName == "Icon");
    }

    [Fact]
    public void Build_DoesNotUseDisplayNameAsIconValue()
    {
        var displayName = @"C:\NotAnIcon\Череп.ico";
        var entry = CreateEntry("entry-skull", displayName);
        entry.IconPath = string.Empty;

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(entry));

        Assert.DoesNotContain(plan.ValueOperations, operation => operation.ValueName == "Icon" && operation.ValueData == displayName);
        Assert.Contains(plan.ValueOperations, operation => operation.ValueName == "MUIVerb" && operation.ValueData == displayName);
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
        Assert.DoesNotContain(plan.ValueOperations, operation => operation.KeyPath.Contains(displayName, StringComparison.Ordinal));
        Assert.Contains(plan.ValueOperations, operation => operation.ValueName == "MUIVerb" && operation.ValueData == displayName);
    }

    [Fact]
    public void Build_PutsGroupedEntryUnderGroupSubmenu()
    {
        var entry = CreateEntry("entry-blue", "Синяя");
        entry.GroupName = "Цветные";

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(entry));

        Assert.Contains(
            plan.KeyOperations,
            operation => operation.KeyPath == $@"{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}\shell\group-001");
        Assert.Contains(
            plan.KeyOperations,
            operation => operation.KeyPath == $@"{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}\shell\group-001\shell\entry-001-entry-blue");
        Assert.Contains(
            plan.ValueOperations,
            operation => operation.KeyPath == $@"{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}\shell\group-001"
                         && operation.ValueName == "MUIVerb"
                         && operation.ValueData == "Цветные");
    }

    [Fact]
    public void Build_DoesNotUseGroupNameAsRegistryKeyName()
    {
        var groupName = "💀 Готические";
        var entry = CreateEntry("entry-skull", "Череп");
        entry.GroupName = groupName;

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(entry));

        Assert.DoesNotContain(plan.KeyOperations, operation => operation.KeyPath.Contains(groupName, StringComparison.Ordinal));
        Assert.DoesNotContain(plan.ValueOperations, operation => operation.KeyPath.Contains(groupName, StringComparison.Ordinal));
        Assert.Contains(plan.ValueOperations, operation => operation.ValueName == "MUIVerb" && operation.ValueData == groupName);
    }

    [Fact]
    public void Build_DuplicateGroupNamesProduceOneGroupSubmenu()
    {
        var first = CreateEntry("entry-blue", "Синяя");
        first.GroupName = "Цветные";
        var second = CreateEntry("entry-red", "Красная");
        second.GroupName = "Цветные";

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(first, second));

        Assert.Single(plan.KeyOperations, operation => operation.KeyPath == $@"{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}\shell\group-001");
        Assert.Contains(plan.ValueOperations, operation => operation.ValueData.Contains("entry-blue"));
        Assert.Contains(plan.ValueOperations, operation => operation.ValueData.Contains("entry-red"));
    }

    [Fact]
    public void Build_CaseDifferentGroupNamesProduceSeparateGroupSubmenus()
    {
        var first = CreateEntry("entry-work", "Work");
        first.GroupName = "Work";
        first.SortOrder = 0;
        var second = CreateEntry("entry-lower-work", "work");
        second.GroupName = "work";
        second.SortOrder = 1;

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettingsPreservingSortOrder(first, second));

        var groupVerbs = plan.ValueOperations
            .Where(operation => operation.ValueName == "MUIVerb"
                                && operation.KeyPath.StartsWith(
                                    $@"{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}\shell\group-",
                                    StringComparison.Ordinal)
                                && operation.KeyPath.IndexOf('\\', ExplorerMenuRegistryPaths.DirectoryBackgroundRoot.Length + @"\shell\group-".Length) < 0)
            .Select(operation => operation.ValueData)
            .ToArray();
        Assert.Equal(["Work", "work"], groupVerbs);
    }

    [Fact]
    public void Build_MaterializesRootEntriesInSortOrder()
    {
        var third = CreateEntry("entry-third", "Третий");
        third.SortOrder = 30;
        var first = CreateEntry("entry-first", "Первый");
        first.SortOrder = 10;
        var second = CreateEntry("entry-second", "Второй");
        second.SortOrder = 20;

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettingsPreservingSortOrder(third, first, second));

        Assert.Equal(["entry-first", "entry-second", "entry-third"], GetCommandEntryIds(plan));
    }

    [Fact]
    public void Build_MaterializesGroupedEntriesInSortOrder()
    {
        var third = CreateEntry("entry-third", "Третий");
        third.GroupName = "Работа";
        third.SortOrder = 30;
        var first = CreateEntry("entry-first", "Первый");
        first.GroupName = "Работа";
        first.SortOrder = 10;
        var second = CreateEntry("entry-second", "Второй");
        second.GroupName = "Работа";
        second.SortOrder = 20;

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettingsPreservingSortOrder(third, first, second));

        Assert.Equal(["entry-first", "entry-second", "entry-third"], GetCommandEntryIds(plan));
    }

    [Fact]
    public void Build_MaterializesRootEntriesBeforeGroupsAndOrdersGroupsByMinimumSortOrder()
    {
        var laterRoot = CreateEntry("entry-later-root", "Поздний root");
        laterRoot.SortOrder = 50;
        var work = CreateEntry("entry-work", "Работа");
        work.GroupName = "Work";
        work.SortOrder = 20;
        var media = CreateEntry("entry-media", "Медиа");
        media.GroupName = "Media";
        media.SortOrder = 10;
        var earlyRoot = CreateEntry("entry-early-root", "Ранний root");
        earlyRoot.SortOrder = 0;

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettingsPreservingSortOrder(laterRoot, work, media, earlyRoot));

        Assert.Equal(
            ["entry-001-entry-early-root", "entry-002-entry-later-root", "group-001", "group-002"],
            GetDirectShellChildNames(plan, ExplorerMenuRegistryPaths.DirectoryBackgroundRoot));
        Assert.Contains(plan.ValueOperations, operation => operation.KeyPath.EndsWith(@"\group-001", StringComparison.Ordinal) && operation.ValueData == "Media");
        Assert.Contains(plan.ValueOperations, operation => operation.KeyPath.EndsWith(@"\group-002", StringComparison.Ordinal) && operation.ValueData == "Work");
    }

    [Fact]
    public void Build_DisabledGroupedEntryDoesNotCreateGroup()
    {
        var entry = CreateEntry("entry-blue", "Синяя");
        entry.GroupName = "Цветные";
        entry.IsEnabled = false;

        var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(entry));

        Assert.Empty(plan.KeyOperations);
        Assert.Empty(plan.ValueOperations);
    }

    [Fact]
    public void Build_EntryIconValueWorksInsideGroup()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraPlanGroupedIcon-");

        try
        {
            var iconPath = Path.Combine(root.FullName, "entry-blue.ico");
            File.WriteAllText(iconPath, "fake icon");
            var entry = CreateEntry("entry-blue", "Синяя");
            entry.GroupName = "Цветные";
            entry.IconPath = iconPath;

            var plan = BuildPlan(ExplorerMenuTargetKind.DirectoryBackground, CreateSettings(entry));

            Assert.Contains(
                plan.ValueOperations,
                operation => operation.KeyPath == $@"{ExplorerMenuRegistryPaths.DirectoryBackgroundRoot}\shell\group-001\shell\entry-001-entry-blue"
                             && operation.ValueName == "Icon"
                             && operation.ValueData == iconPath);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Theory]
    [InlineData(ExplorerMenuTargetKind.Directory)]
    [InlineData(ExplorerMenuTargetKind.DirectoryBackground)]
    public void Build_SupportsGroupsForBothTargets(ExplorerMenuTargetKind targetKind)
    {
        var entry = CreateEntry("entry-blue", "Синяя");
        entry.GroupName = "Цветные";

        var plan = BuildPlan(targetKind, CreateSettings(entry));
        var root = ExplorerMenuRegistryPaths.GetOwnedRoot(targetKind);

        Assert.Contains(plan.KeyOperations, operation => operation.KeyPath == $@"{root}\shell\group-001");
        Assert.Contains(plan.KeyOperations, operation => operation.KeyPath == $@"{root}\shell\group-001\shell\entry-001-entry-blue");
    }

    [Fact]
    public void Build_QuotesCommandHostPathWithSpacesAndCyrillic()
    {
        var hostPath = @"C:\Program Files\Фолдора\Foldora.MenuHost.exe";

        var plan = new ExplorerMenuRegistryPlanBuilder().Build(
            hostPath,
            CreateSettings(CreateEntry("entry-skull", "Череп")),
            ExplorerMenuTargetKind.DirectoryBackground);

        var command = Assert.Single(plan.ValueOperations, operation => operation.KeyPath.EndsWith(@"\command") && operation.ValueName == string.Empty);
        Assert.StartsWith(@"""C:\Program Files\Фолдора\Foldora.MenuHost.exe""", command.ValueData);
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
            @"C:\Program Files\Foldora\Foldora.MenuHost.exe",
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

    private static FolderMenuSettings CreateSettingsPreservingSortOrder(params FolderMenuEntry[] entries)
    {
        var settings = new FolderMenuSettings();
        settings.Entries.AddRange(entries);
        return settings;
    }

    private static string[] GetCommandEntryIds(ExplorerMenuRegistryPlan plan)
    {
        return plan.ValueOperations
            .Where(operation => operation.KeyPath.EndsWith(@"\command", StringComparison.Ordinal)
                                && operation.ValueName == string.Empty)
            .Select(operation => ExtractEntryId(operation.ValueData))
            .ToArray();
    }

    private static string[] GetDirectShellChildNames(ExplorerMenuRegistryPlan plan, string root)
    {
        var rootShell = $@"{root}\shell\";
        return plan.KeyOperations
            .Select(operation => operation.KeyPath)
            .Where(path => path.StartsWith(rootShell, StringComparison.Ordinal))
            .Select(path => path[rootShell.Length..])
            .Where(child => !child.Contains('\\', StringComparison.Ordinal))
            .ToArray();
    }

    private static string ExtractEntryId(string command)
    {
        const string marker = "--entry-id";
        var markerIndex = command.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(markerIndex >= 0);
        var value = command[(markerIndex + marker.Length)..].Trim();
        Assert.StartsWith("\"", value, StringComparison.Ordinal);
        var endIndex = value.IndexOf('"', 1);
        Assert.True(endIndex > 1);
        return value[1..endIndex];
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
