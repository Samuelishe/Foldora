using Foldora.Cli;
using Foldora.Core.DesktopIni;
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
            await new DesktopIniService().ApplyIconAsync(
                new DesktopIniOptions(parsedCommand.FolderPath!, parsedCommand.IconPath!));
            Console.WriteLine("Folder icon was applied. Explorer may not refresh the icon immediately.");
            return 0;

        case CliCommandKind.Clear:
            await new DesktopIniService().ClearIconAsync(parsedCommand.FolderPath!);
            Console.WriteLine("Folder icon entry was cleared. Explorer may not refresh the icon immediately.");
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
  foldora clear --folder "<folder>"
  foldora create --target "<directory>" --style "<style-id>"
  foldora import-pack --path "<pack-path>"
  foldora list-packs
  foldora list-styles
  foldora register-menu
  foldora unregister-menu
  foldora settings

Implemented now:
  apply --folder --icon
  clear --folder

The --style flow, pack import, registry context menu, Explorer restart, and icon cache reset are not implemented in this step.
""");
}
