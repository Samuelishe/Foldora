using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.MenuHost;
using Foldora.Shell.Desktop;
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

    [Fact]
    public async Task RunCreate_CapturesCursorBeforeCreateAction()
    {
        var calls = new List<string>();
        var actionService = new FakeMenuHostFolderActionService(
            (_, _, _) =>
            {
                calls.Add("create");
                return Task.FromResult(@"C:\Users\User\Desktop\Череп");
            });
        var cursorProvider = new FakeCursorPositionProvider(new CursorPosition(100, 200), calls);
        var positioningService = new FakeDesktopIconPositioningService();
        var coordinator = CreateCoordinator(@"C:\Users\User\Desktop", positioningService);

        var exitCode = await MenuHostCommandRunner.RunAsync(
            ["create", "--target", @"C:\Users\User\Desktop", "--entry-id", "entry-skull"],
            actionService,
            cursorProvider,
            coordinator);

        Assert.Equal(0, exitCode);
        Assert.Equal(["cursor", "create"], calls);
    }

    [Fact]
    public async Task RunCreate_PositionsCreatedFolderForDesktopTarget()
    {
        var actionService = new FakeMenuHostFolderActionService(
            (_, _, _) => Task.FromResult(@"C:\Users\User\Desktop\Череп"));
        var cursorProvider = new FakeCursorPositionProvider(new CursorPosition(320, 240));
        var positioningService = new FakeDesktopIconPositioningService();
        var coordinator = CreateCoordinator(@"C:\Users\User\Desktop", positioningService);

        var exitCode = await MenuHostCommandRunner.RunAsync(
            ["create", "--target", @"C:\Users\User\Desktop\", "--entry-id", "entry-skull"],
            actionService,
            cursorProvider,
            coordinator);

        Assert.Equal(0, exitCode);
        Assert.Single(positioningService.Calls);
        var call = positioningService.Calls[0];
        Assert.Equal("Череп", call.ItemName);
        Assert.Equal(320, call.X);
        Assert.Equal(240, call.Y);
        Assert.Equal(DesktopIconCoordinateSpace.Screen, call.CoordinateSpace);
    }

    [Fact]
    public async Task RunCreate_DoesNotPositionForNonDesktopTarget()
    {
        var actionService = new FakeMenuHostFolderActionService(
            (_, _, _) => Task.FromResult(@"C:\Temp\Череп"));
        var cursorProvider = new FakeCursorPositionProvider(new CursorPosition(320, 240));
        var positioningService = new FakeDesktopIconPositioningService();
        var coordinator = CreateCoordinator(@"C:\Users\User\Desktop", positioningService);

        var exitCode = await MenuHostCommandRunner.RunAsync(
            ["create", "--target", @"C:\Temp", "--entry-id", "entry-skull"],
            actionService,
            cursorProvider,
            coordinator);

        Assert.Equal(0, exitCode);
        Assert.Empty(positioningService.Calls);
    }

    [Fact]
    public async Task RunCreate_PositioningFailureDoesNotFailSuccessfulCreate()
    {
        var actionService = new FakeMenuHostFolderActionService(
            (_, _, _) => Task.FromResult(@"C:\Users\User\Desktop\Череп"));
        var cursorProvider = new FakeCursorPositionProvider(new CursorPosition(320, 240));
        var positioningService = new FakeDesktopIconPositioningService(
            DesktopIconPositioningResult.Failed("Explorer rejected positioning."));
        var coordinator = CreateCoordinator(@"C:\Users\User\Desktop", positioningService);

        var exitCode = await MenuHostCommandRunner.RunAsync(
            ["create", "--target", @"C:\Users\User\Desktop", "--entry-id", "entry-skull"],
            actionService,
            cursorProvider,
            coordinator);

        Assert.Equal(0, exitCode);
        Assert.Single(positioningService.Calls);
    }

    [Fact]
    public async Task RunCreate_PositioningExceptionDoesNotFailSuccessfulCreate()
    {
        var actionService = new FakeMenuHostFolderActionService(
            (_, _, _) => Task.FromResult(@"C:\Users\User\Desktop\Череп"));
        var cursorProvider = new FakeCursorPositionProvider(new CursorPosition(320, 240));
        var positioningService = new ThrowingDesktopIconPositioningService();
        var coordinator = CreateCoordinator(@"C:\Users\User\Desktop", positioningService);

        var exitCode = await MenuHostCommandRunner.RunAsync(
            ["create", "--target", @"C:\Users\User\Desktop", "--entry-id", "entry-skull"],
            actionService,
            cursorProvider,
            coordinator);

        Assert.Equal(0, exitCode);
        Assert.Equal(1, positioningService.CallCount);
    }

    [Fact]
    public async Task RunCreate_DoesNotPositionWhenCreateFails()
    {
        var actionService = new FakeMenuHostFolderActionService(
            (_, _, _) => throw new InvalidOperationException("Create failed."));
        var cursorProvider = new FakeCursorPositionProvider(new CursorPosition(320, 240));
        var positioningService = new FakeDesktopIconPositioningService();
        var coordinator = CreateCoordinator(@"C:\Users\User\Desktop", positioningService);

        var exitCode = await MenuHostCommandRunner.RunAsync(
            ["create", "--target", @"C:\Users\User\Desktop", "--entry-id", "entry-skull"],
            actionService,
            cursorProvider,
            coordinator);

        Assert.Equal(1, exitCode);
        Assert.Empty(positioningService.Calls);
    }

    private static async Task SaveSettingsAsync(FoldoraDataPaths paths, FolderMenuEntry entry)
    {
        var settings = new FoldoraSettings();
        settings.CreateFolderMenu.Entries.Add(entry);
        await new FoldoraSettingsStorage(paths).SaveAsync(settings);
    }

    private static DesktopPlacementCoordinator CreateCoordinator(
        string desktopPath,
        IDesktopIconPositioningService positioningService)
    {
        return new DesktopPlacementCoordinator(
            new DesktopTargetDetector(() => desktopPath),
            positioningService);
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

    private sealed class FakeMenuHostFolderActionService : IMenuHostFolderActionService
    {
        private readonly Func<string, string, CancellationToken, Task<string>> create;

        public FakeMenuHostFolderActionService(Func<string, string, CancellationToken, Task<string>> create)
        {
            this.create = create;
        }

        public Task<string> CreateAsync(
            string targetDirectory,
            string entryId,
            CancellationToken cancellationToken = default)
        {
            return create(targetDirectory, entryId, cancellationToken);
        }

        public Task ApplyAsync(
            string folderPath,
            string entryId,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCursorPositionProvider : ICursorPositionProvider
    {
        private readonly CursorPosition? cursorPosition;
        private readonly ICollection<string>? calls;

        public FakeCursorPositionProvider(CursorPosition? cursorPosition, ICollection<string>? calls = null)
        {
            this.cursorPosition = cursorPosition;
            this.calls = calls;
        }

        public CursorPosition? TryGetCursorPosition()
        {
            calls?.Add("cursor");
            return cursorPosition;
        }
    }

    private sealed class FakeDesktopIconPositioningService : IDesktopIconPositioningService
    {
        private readonly DesktopIconPositioningResult result;

        public FakeDesktopIconPositioningService()
            : this(DesktopIconPositioningResult.Succeeded("Moved."))
        {
        }

        public FakeDesktopIconPositioningService(DesktopIconPositioningResult result)
        {
            this.result = result;
        }

        public List<PositionCall> Calls { get; } = [];

        public DesktopIconPositioningResult TryPositionByName(
            string itemName,
            int x,
            int y,
            DesktopIconCoordinateSpace coordinateSpace)
        {
            Calls.Add(new PositionCall(itemName, x, y, coordinateSpace));
            return result;
        }
    }

    private sealed record PositionCall(
        string ItemName,
        int X,
        int Y,
        DesktopIconCoordinateSpace CoordinateSpace);

    private sealed class ThrowingDesktopIconPositioningService : IDesktopIconPositioningService
    {
        public int CallCount { get; private set; }

        public DesktopIconPositioningResult TryPositionByName(
            string itemName,
            int x,
            int y,
            DesktopIconCoordinateSpace coordinateSpace)
        {
            CallCount++;
            throw new InvalidOperationException("Positioning failed.");
        }
    }
}
