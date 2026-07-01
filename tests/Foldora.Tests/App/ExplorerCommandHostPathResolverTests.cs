using Foldora.App.Services;
using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Shell.Registry;
using Foldora.Shell.RegistryPlan;
using Foldora.Tests.Shell.Fakes;

namespace Foldora.Tests.App;

public sealed class ExplorerCommandHostPathResolverTests
{
    [Fact]
    public async Task ResolveCommandHostPath_FindsSiblingMenuHostNextToAppExecutable()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraHostResolver-");

        try
        {
            var publishDirectory = Directory.CreateDirectory(Path.Combine(root.FullName, "artifacts", "publish", "Foldora"));
            var appPath = Path.Combine(publishDirectory.FullName, "Foldora.App.exe");
            var hostPath = Path.Combine(publishDirectory.FullName, "Foldora.MenuHost.exe");
            await File.WriteAllTextAsync(appPath, "fake app");
            await File.WriteAllTextAsync(hostPath, "fake host");
            var resolver = new ExplorerCommandHostPathResolver(() => appPath);

            var resolvedPath = resolver.ResolveCommandHostPath();

            Assert.Equal(hostPath, resolvedPath);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ResolveCommandHostPath_FindsInstalledSiblingMenuHostInPathWithSpaces()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraHostResolver-");

        try
        {
            var installDirectory = Directory.CreateDirectory(Path.Combine(root.FullName, "Local AppData", "Programs", "Foldora"));
            var appPath = Path.Combine(installDirectory.FullName, "Foldora.App.exe");
            var hostPath = Path.Combine(installDirectory.FullName, "Foldora.MenuHost.exe");
            await File.WriteAllTextAsync(appPath, "fake app");
            await File.WriteAllTextAsync(hostPath, "fake host");
            var resolver = new ExplorerCommandHostPathResolver(() => appPath);

            var resolvedPath = resolver.ResolveCommandHostPath();

            Assert.Equal(hostPath, resolvedPath);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ResolveCommandHostPath_ReportsControlledFailureWhenMenuHostIsMissing()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraHostResolver-");

        try
        {
            var publishDirectory = Directory.CreateDirectory(Path.Combine(root.FullName, "artifacts", "publish", "Foldora"));
            var appPath = Path.Combine(publishDirectory.FullName, "Foldora.App.exe");
            await File.WriteAllTextAsync(appPath, "fake app");
            var resolver = new ExplorerCommandHostPathResolver(() => appPath);

            var exception = Assert.Throws<FileNotFoundException>(() => resolver.ResolveCommandHostPath());

            Assert.Contains("Foldora.MenuHost.exe was not found", exception.Message, StringComparison.Ordinal);
            Assert.EndsWith("Foldora.MenuHost.exe", exception.FileName, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ResolveCommandHostPath_FindsDevelopmentMenuHostFromAppBuildOutput()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraHostResolver-");

        try
        {
            var appDirectory = Directory.CreateDirectory(Path.Combine(root.FullName, "src", "Foldora.App", "bin", "Debug", "net10.0-windows"));
            var hostDirectory = Directory.CreateDirectory(Path.Combine(root.FullName, "src", "Foldora.MenuHost", "bin", "Debug", "net10.0-windows"));
            var appPath = Path.Combine(appDirectory.FullName, "Foldora.App.exe");
            var hostPath = Path.Combine(hostDirectory.FullName, "Foldora.MenuHost.exe");
            await File.WriteAllTextAsync(appPath, "fake app");
            await File.WriteAllTextAsync(hostPath, "fake host");
            var resolver = new ExplorerCommandHostPathResolver(() => appPath);

            var resolvedPath = resolver.ResolveCommandHostPath();

            Assert.Equal(hostPath, resolvedPath);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task Register_UsesResolvedSiblingMenuHostPathInRegistryCommand()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraHostResolver-");

        try
        {
            var publishDirectory = Directory.CreateDirectory(Path.Combine(root.FullName, "artifacts", "publish", "Foldora"));
            var appPath = Path.Combine(publishDirectory.FullName, "Foldora.App.exe");
            var hostPath = Path.Combine(publishDirectory.FullName, "Foldora.MenuHost.exe");
            await File.WriteAllTextAsync(appPath, "fake app");
            await File.WriteAllTextAsync(hostPath, "fake host");

            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "AppData", "Foldora"));
            await SaveSettingsAsync(paths, CreateEntry("entry-blue", "Синяя"));
            var registry = new FakeRegistryAccess();
            var storage = new FoldoraSettingsStorage(paths);
            var editor = new FolderMenuDraftEditor(storage, paths);
            await editor.LoadAsync();
            var registrationService = new ExplorerMenuRegistrationService(
                storage,
                new ExplorerMenuRegistryPlanBuilder(),
                new ExplorerMenuRegistryWriter(registry));
            var controller = new ExplorerIntegrationController(
                editor,
                registrationService,
                new ExplorerCommandHostPathResolver(() => appPath));

            var result = await controller.RegisterAsync();

            Assert.True(result.Success);
            Assert.Contains(
                registry.Values.Values,
                value => value.Contains(hostPath, StringComparison.Ordinal)
                         && value.Contains("--entry-id", StringComparison.Ordinal));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    private static async Task SaveSettingsAsync(FoldoraDataPaths paths, params FolderMenuEntry[] entries)
    {
        var settings = new FoldoraSettings();
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
            DefaultFolderName = displayName,
            IconPath = $@"C:\Foldora\icons\{id}.ico",
            IsEnabled = true
        };
    }
}
