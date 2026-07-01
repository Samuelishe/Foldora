using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Shell.Desktop;

namespace Foldora.MenuHost;

/// <summary>
/// Выполняет команды Explorer menu без UI и без console output.
/// </summary>
public static class MenuHostCommandRunner
{
    public static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        return await RunAsync(
            args,
            new MenuHostFolderActionService(CreateActionService()),
            new WindowsCursorPositionProvider(),
            new DesktopPlacementCoordinator(
                new DesktopTargetDetector(),
                new WindowsDesktopIconPositioningService()),
            new MenuHostPlacementLogWriter(),
            cancellationToken);
    }

    public static async Task<int> RunAsync(
        string[] args,
        FolderMenuEntryActionService actionService,
        CancellationToken cancellationToken = default)
    {
        return await RunAsync(
            args,
            new MenuHostFolderActionService(actionService),
            new NullCursorPositionProvider(),
            new DesktopPlacementCoordinator(
                new DesktopTargetDetector(() => string.Empty),
                new NoOpDesktopIconPositioningService()),
            new NoOpMenuHostPlacementLogWriter(),
            cancellationToken);
    }

    internal static async Task<int> RunAsync(
        string[] args,
        IMenuHostFolderActionService actionService,
        ICursorPositionProvider cursorPositionProvider,
        DesktopPlacementCoordinator desktopPlacementCoordinator,
        IMenuHostPlacementLogWriter placementLogWriter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(actionService);
        ArgumentNullException.ThrowIfNull(cursorPositionProvider);
        ArgumentNullException.ThrowIfNull(desktopPlacementCoordinator);
        ArgumentNullException.ThrowIfNull(placementLogWriter);

        try
        {
            var command = MenuHostCommandParser.Parse(args);
            if (!command.IsValid)
            {
                return 1;
            }
            switch (command.Kind)
            {
                case MenuHostCommandKind.Create:
                    return await RunCreateAsync(
                        command,
                        actionService,
                        cursorPositionProvider,
                        desktopPlacementCoordinator,
                        placementLogWriter,
                        cancellationToken);

                case MenuHostCommandKind.Apply:
                    await actionService.ApplyAsync(command.FolderPath!, command.EntryId!, cancellationToken);
                    return 0;

                case MenuHostCommandKind.Unknown:
                default:
                    return 1;
            }
        }
        catch (Exception exception) when (IsExpectedFailure(exception))
        {
            return 1;
        }
    }

    private static async Task<int> RunCreateAsync(
        MenuHostCommand command,
        IMenuHostFolderActionService actionService,
        ICursorPositionProvider cursorPositionProvider,
        DesktopPlacementCoordinator desktopPlacementCoordinator,
        IMenuHostPlacementLogWriter placementLogWriter,
        CancellationToken cancellationToken)
    {
        var logEntry = new MenuHostPlacementLogEntry
        {
            CommandKind = "create",
            TargetPath = command.TargetPath,
            EntryId = command.EntryId
        };

        try
        {
            var cursorPosition = cursorPositionProvider.TryGetCursorPosition();
            logEntry.CursorCaptured = cursorPosition is not null;
            logEntry.CursorX = cursorPosition?.X;
            logEntry.CursorY = cursorPosition?.Y;

            var createdFolderPath = await actionService.CreateAsync(
                command.TargetPath!,
                command.EntryId!,
                cancellationToken);
            logEntry.CreatedFolderPath = createdFolderPath;

            var placementResult = await desktopPlacementCoordinator.TryPlaceCreatedFolderAsync(
                command.TargetPath!,
                createdFolderPath,
                cursorPosition,
                cancellationToken);

            logEntry.ApplyPlacementResult(placementResult);
            logEntry.FinalExitCode = 0;
            return 0;
        }
        catch (Exception exception) when (IsExpectedFailure(exception))
        {
            logEntry.ExceptionType = exception.GetType().FullName;
            logEntry.ExceptionMessage = exception.Message;
            logEntry.FinalPositioningResult = "skipped";
            logEntry.FinalPositioningMessage = "Create failed before placement.";
            logEntry.FinalExitCode = 1;
            return 1;
        }
        finally
        {
            try
            {
                placementLogWriter.Append(logEntry);
            }
            catch
            {
                // Explorer create/apply results must not depend on diagnostic logging.
            }
        }
    }

    private sealed class NullCursorPositionProvider : ICursorPositionProvider
    {
        public CursorPosition? TryGetCursorPosition()
        {
            return null;
        }
    }

    private sealed class NoOpDesktopIconPositioningService : IDesktopIconPositioningService
    {
        public DesktopIconPositioningResult TryPositionByName(
            string itemName,
            int x,
            int y,
            DesktopIconCoordinateSpace coordinateSpace)
        {
            return DesktopIconPositioningResult.Failed("Desktop placement is disabled for this runner overload.");
        }
    }

    private sealed class NoOpMenuHostPlacementLogWriter : IMenuHostPlacementLogWriter
    {
        public void Append(MenuHostPlacementLogEntry entry)
        {
        }
    }

    private static FolderMenuEntryActionService CreateActionService()
    {
        var paths = FoldoraDataPaths.CreateDefault();
        var storage = new FoldoraSettingsStorage(paths);
        return new FolderMenuEntryActionService(storage);
    }

    private static bool IsExpectedFailure(Exception exception)
    {
        return exception is DirectoryNotFoundException
            or FileNotFoundException
            or InvalidOperationException
            or ArgumentException
            or UnauthorizedAccessException
            or IOException
            or System.Security.SecurityException;
    }
}
