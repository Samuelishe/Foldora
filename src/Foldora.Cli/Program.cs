using Foldora.Cli;
using Foldora.Core.DesktopIni;
using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Shell.ContextMenu;

var parsedCommand = CliCommandParser.Parse(args);

if (!parsedCommand.IsValid)
{
    Console.Error.WriteLine(parsedCommand.Error);
    PrintHelp();
    return 1;
}

try
{
    switch (parsedCommand.Kind)
    {
        case CliCommandKind.Help:
            PrintHelp();
            return 0;

        case CliCommandKind.Apply:
            if (parsedCommand.EntryId is not null)
            {
                await CreateEntryActionService().ApplyAsync(parsedCommand.FolderPath!, parsedCommand.EntryId);
            }
            else
            {
                await new DesktopIniService().ApplyIconAsync(
                    new DesktopIniOptions(parsedCommand.FolderPath!, parsedCommand.IconPath!));
            }

            Console.WriteLine("Folder icon was applied. Explorer may not refresh the icon immediately.");
            return 0;

        case CliCommandKind.Create:
            var createdFolderPath = await CreateEntryActionService().CreateAsync(
                parsedCommand.TargetPath!,
                parsedCommand.EntryId!);
            Console.WriteLine($"Folder was created: {createdFolderPath}");
            Console.WriteLine("Explorer may not refresh the icon immediately.");
            return 0;

        case CliCommandKind.Clear:
            await new DesktopIniService().ClearIconAsync(parsedCommand.FolderPath!);
            Console.WriteLine("Folder icon entry was cleared. Explorer may not refresh the icon immediately.");
            return 0;

        case CliCommandKind.MenuList:
            await ListMenuAsync();
            return 0;

        case CliCommandKind.MenuAdd:
            await AddMenuEntryAsync(parsedCommand.IconPath!, parsedCommand.DisplayName, parsedCommand.DefaultFolderName);
            return 0;

        case CliCommandKind.MenuRemove:
            await RemoveMenuEntryAsync(parsedCommand.EntryId!);
            return 0;

        case CliCommandKind.Skeleton:
            Console.WriteLine($"Command '{parsedCommand.Name}' is a skeleton and is not implemented yet.");
            return 0;

        case CliCommandKind.RegisterMenu:
            Console.WriteLine("Command 'register-menu' is planned for a separate step. Registry writes are not implemented now.");
            return 0;

        case CliCommandKind.UnregisterMenu:
            Console.WriteLine("Command 'unregister-menu' is planned for a separate step. Registry writes are not implemented now.");
            return 0;

        case CliCommandKind.Quote:
            Console.WriteLine(CommandLineQuoter.Quote(parsedCommand.QuoteValue ?? string.Empty));
            return 0;

        case CliCommandKind.Unknown:
        default:
            Console.Error.WriteLine($"Unknown command '{parsedCommand.Name}'.");
            PrintHelp();
            return 1;
    }
}
catch (Exception exception) when (exception is DirectoryNotFoundException
                                  or FileNotFoundException
                                  or InvalidOperationException
                                  or ArgumentException
                                  or UnauthorizedAccessException
                                  or IOException)
{
    Console.Error.WriteLine($"Error: {exception.Message}");
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine("""
Foldora CLI

Usage:
  foldora apply --folder "<folder>" --icon "<absolute-icon-path>"
  foldora apply --folder "<folder>" --entry-id "<entry-id>"
  foldora create --target "<directory>" --entry-id "<entry-id>"
  foldora clear --folder "<folder>"
  foldora menu list
  foldora menu add --icon "<absolute-icon-path>" [--name "<display-name>"] [--folder-name "<default-folder-name>"]
  foldora menu remove --entry-id "<entry-id>"
  foldora import-pack --path "<pack-path>"
  foldora list-packs
  foldora list-styles
  foldora register-menu
  foldora unregister-menu
  foldora settings

Implemented now:
  apply --folder --icon
  apply --folder --entry-id
  create --target --entry-id
  clear --folder
  menu list
  menu add --icon [--name] [--folder-name]
  menu remove --entry-id

The --style flow, pack import, registry context menu, Explorer restart, and icon cache reset are not implemented in this step.
""");
}

static FolderMenuService CreateFolderMenuService()
{
    var paths = FoldoraDataPaths.CreateDefault();
    var storage = new FoldoraSettingsStorage(paths);
    return new FolderMenuService(storage, paths);
}

static FolderMenuEntryActionService CreateEntryActionService()
{
    var paths = FoldoraDataPaths.CreateDefault();
    var storage = new FoldoraSettingsStorage(paths);
    return new FolderMenuEntryActionService(storage);
}

static async Task ListMenuAsync()
{
    var entries = await CreateFolderMenuService().ListAsync();

    if (entries.Count == 0)
    {
        Console.WriteLine("No menu entries configured.");
        return;
    }

    foreach (var entry in entries)
    {
        var enabledState = entry.IsEnabled ? "enabled" : "disabled";
        Console.WriteLine($"{entry.Id}\t{entry.DisplayName}\t{entry.DefaultFolderName}\t{enabledState}\t{entry.IconPath}");
    }
}

static async Task AddMenuEntryAsync(string iconPath, string? displayName, string? defaultFolderName)
{
    var entry = await CreateFolderMenuService().AddAsync(iconPath, displayName, defaultFolderName);

    Console.WriteLine("Menu entry added.");
    Console.WriteLine($"EntryId: {entry.Id}");
    Console.WriteLine($"DisplayName: {entry.DisplayName}");
    Console.WriteLine($"DefaultFolderName: {entry.DefaultFolderName}");
    Console.WriteLine($"IconPath: {entry.IconPath}");
}

static async Task RemoveMenuEntryAsync(string entryId)
{
    var entry = await CreateFolderMenuService().RemoveAsync(entryId);

    Console.WriteLine("Menu entry removed.");
    Console.WriteLine($"EntryId: {entry.Id}");
    Console.WriteLine($"DisplayName: {entry.DisplayName}");
}
