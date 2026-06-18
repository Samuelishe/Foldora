using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.MenuHost;
using Foldora.Tests.Fixtures;

namespace Foldora.Tests.MenuHost;

public sealed class MenuHostCommandRunnerTests
{
    [Fact]
    public void Project_BuildsAsWindowsSubsystemExecutable()
    {
        var repositoryRoot = FindRepositoryRoot();
        var projectFile = Path.Combine(repositoryRoot, "src", "Foldora.MenuHost", "Foldora.MenuHost.csproj");
        var projectText = File.ReadAllText(projectFile);

        Assert.Contains("<OutputType>WinExe</OutputType>", projectText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<TargetFramework>net10.0-windows</TargetFramework>", projectText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunCreate_InvokesExistingActionServiceAndCreatesFolder()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraMenuHost-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var target = Path.Combine(root.FullName, "Target");
            Directory.CreateDirectory(target);
            var iconPath = Path.Combine(paths.IconsDirectory, "entry-skull.ico");
            Directory.CreateDirectory(paths.IconsDirectory);
            await IcoTestFile.WriteValidAsync(iconPath);
            await SaveSettingsAsync(paths, CreateEntry("entry-skull", "Череп", iconPath));
            var actionService = new FolderMenuEntryActionService(new FoldoraSettingsStorage(paths));

            var exitCode = await MenuHostCommandRunner.RunAsync(
                ["create", "--target", target, "--entry-id", "entry-skull"],
                actionService);

            Assert.Equal(0, exitCode);
            var createdFolder = new DirectoryInfo(Path.Combine(target, "Череп"));
            Assert.True(createdFolder.Exists);
            Assert.True(File.Exists(Path.Combine(createdFolder.FullName, "desktop.ini")));
            Assert.True(createdFolder.Attributes.HasFlag(FileAttributes.ReadOnly));
            Assert.False(createdFolder.Attributes.HasFlag(FileAttributes.System));
        }
        finally
        {
            ClearAttributes(root);
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RunCreate_ReturnsFailureForUnknownEntry()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraMenuHost-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var target = Path.Combine(root.FullName, "Target");
            Directory.CreateDirectory(target);
            await new FoldoraSettingsStorage(paths).SaveAsync(new FoldoraSettings());
            var actionService = new FolderMenuEntryActionService(new FoldoraSettingsStorage(paths));

            var exitCode = await MenuHostCommandRunner.RunAsync(
                ["create", "--target", target, "--entry-id", "missing-entry"],
                actionService);

            Assert.Equal(1, exitCode);
            Assert.Empty(Directory.EnumerateFileSystemEntries(target));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task RunCreate_ReturnsFailureForMissingTarget()
    {
        var root = Directory.CreateTempSubdirectory("FoldoraMenuHost-");

        try
        {
            var paths = new FoldoraDataPaths(Path.Combine(root.FullName, "Foldora"));
            var iconPath = Path.Combine(paths.IconsDirectory, "entry-skull.ico");
            Directory.CreateDirectory(paths.IconsDirectory);
            await IcoTestFile.WriteValidAsync(iconPath);
            await SaveSettingsAsync(paths, CreateEntry("entry-skull", "Череп", iconPath));
            var actionService = new FolderMenuEntryActionService(new FoldoraSettingsStorage(paths));

            var exitCode = await MenuHostCommandRunner.RunAsync(
                ["create", "--target", Path.Combine(root.FullName, "Missing"), "--entry-id", "entry-skull"],
                actionService);

            Assert.Equal(1, exitCode);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    private static async Task SaveSettingsAsync(FoldoraDataPaths paths, FolderMenuEntry entry)
    {
        var settings = new FoldoraSettings();
        settings.CreateFolderMenu.Entries.Add(entry);
        await new FoldoraSettingsStorage(paths).SaveAsync(settings);
    }

    private static FolderMenuEntry CreateEntry(string id, string displayName, string iconPath)
    {
        return new FolderMenuEntry
        {
            Id = id,
            DisplayName = displayName,
            DefaultFolderName = displayName,
            IconPath = iconPath,
            IsEnabled = true
        };
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Foldora.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }

    private static void ClearAttributes(DirectoryInfo directory)
    {
        foreach (var file in directory.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            file.Attributes = FileAttributes.Normal;
        }

        foreach (var childDirectory in directory.EnumerateDirectories("*", SearchOption.AllDirectories))
        {
            childDirectory.Attributes = FileAttributes.Normal;
        }

        directory.Attributes = FileAttributes.Normal;
    }
}
