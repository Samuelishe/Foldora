using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;

namespace Foldora.MenuHost;

/// <summary>
/// Выполняет команды Explorer menu без UI и без console output.
/// </summary>
public static class MenuHostCommandRunner
{
    public static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        return await RunAsync(args, CreateActionService(), cancellationToken);
    }

    public static async Task<int> RunAsync(
        string[] args,
        FolderMenuEntryActionService actionService,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(actionService);

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
                    await actionService.CreateAsync(command.TargetPath!, command.EntryId!, cancellationToken);
                    return 0;

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
