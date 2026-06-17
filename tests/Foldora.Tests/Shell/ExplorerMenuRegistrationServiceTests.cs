using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Shell.Registry;
using Foldora.Shell.RegistryPlan;
using Foldora.Tests.Shell.Fakes;

namespace Foldora.Tests.Shell;

public sealed class ExplorerMenuRegistrationServiceTests
{
    [Fact]
    public async Task RegisterDryRun_BuildsPlanButDoesNotWriteRegistryOrChangeSettings()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraRegister-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: false, CreateEntry("entry-skull", "Череп"));
            var cliPath = await CreateFakeCliAsync(root.FullName);
            var registry = new FakeRegistryAccess();

            var result = await CreateService(paths, registry).RegisterAsync(cliPath, dryRun: true);
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.DryRun);
            Assert.False(result.Applied);
            Assert.Empty(registry.Calls);
            Assert.False(settings.ExplorerIntegrationEnabled);
            Assert.NotEmpty(result.Plans);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Register_AppliesValidatedPlanAndEnablesIntegration()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraRegister-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: false, CreateEntry("entry-skull", "Череп"));
            var cliPath = await CreateFakeCliAsync(root.FullName);
            var registry = new FakeRegistryAccess();

            var result = await CreateService(paths, registry).RegisterAsync(cliPath, dryRun: false);
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Applied);
            Assert.True(settings.ExplorerIntegrationEnabled);
            Assert.All(registry.Keys, key => Assert.Contains(@"\Foldora", key));
            Assert.Contains(registry.Values.Values, value => value.Contains("create") && value.Contains("--entry-id"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Register_WithEmptyEntriesDeletesOwnedKeysAndDisablesIntegration()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraRegister-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: true);
            var cliPath = await CreateFakeCliAsync(root.FullName);
            var registry = new FakeRegistryAccess();
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryRoot);
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, @"Software\Classes\Directory\shell\OtherApp");

            var result = await CreateService(paths, registry).RegisterAsync(cliPath, dryRun: false);
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.True(result.Applied);
            Assert.False(settings.ExplorerIntegrationEnabled);
            Assert.False(registry.ContainsKey(ExplorerMenuRegistryPaths.DirectoryRoot));
            Assert.True(registry.ContainsKey(@"Software\Classes\Directory\shell\OtherApp"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Unregister_DeletesOnlyOwnedKeysAndIsIdempotent()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraRegister-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: true, CreateEntry("entry-skull", "Череп"));
            var registry = new FakeRegistryAccess();
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryRoot);
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryBackgroundRoot);
            registry.CreateKey(ExplorerMenuRegistryHive.CurrentUser, @"Software\Classes\Directory\shell\OtherApp");
            var service = CreateService(paths, registry);

            await service.UnregisterAsync();
            await service.UnregisterAsync();
            var settings = await new FoldoraSettingsStorage(paths).LoadAsync();

            Assert.False(settings.ExplorerIntegrationEnabled);
            Assert.False(registry.ContainsKey(ExplorerMenuRegistryPaths.DirectoryRoot));
            Assert.False(registry.ContainsKey(ExplorerMenuRegistryPaths.DirectoryBackgroundRoot));
            Assert.True(registry.ContainsKey(@"Software\Classes\Directory\shell\OtherApp"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Register_RejectsMissingCliPath()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraRegister-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            await SaveSettingsAsync(paths, explorerIntegrationEnabled: false, CreateEntry("entry-skull", "Череп"));
            var missingCliPath = Path.Combine(root.FullName, "Missing", "Foldora.Cli.exe");

            await Assert.ThrowsAsync<FileNotFoundException>(
                () => CreateService(paths, new FakeRegistryAccess()).RegisterAsync(missingCliPath, dryRun: false));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    private static ExplorerMenuRegistrationService CreateService(FoldoraDataPaths paths, FakeRegistryAccess registry)
    {
        return new ExplorerMenuRegistrationService(
            new FoldoraSettingsStorage(paths),
            new ExplorerMenuRegistryPlanBuilder(),
            new ExplorerMenuRegistryWriter(registry));
    }

    private static async Task SaveSettingsAsync(
        FoldoraDataPaths paths,
        bool explorerIntegrationEnabled,
        params FolderMenuEntry[] entries)
    {
        var settings = new FoldoraSettings { ExplorerIntegrationEnabled = explorerIntegrationEnabled };
        foreach (var entry in entries)
        {
            settings.CreateFolderMenu.Entries.Add(entry);
        }

        await new FoldoraSettingsStorage(paths).SaveAsync(settings);
    }

    private static FolderMenuEntry CreateEntry(string id, string displayName)
    {
        return new FolderMenuEntry
        {
            Id = id,
            DisplayName = displayName,
            DefaultFolderName = "Череп",
            IconPath = $@"C:\Foldora\icons\{id}.ico",
            IsEnabled = true
        };
    }

    private static async Task<string> CreateFakeCliAsync(string root)
    {
        var path = Path.Combine(root, "Program Files", "Фолдора", "Foldora.Cli.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, "fake exe");
        return path;
    }
}
