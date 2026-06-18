using Foldora.Cli;
using Foldora.Core.DesktopIni;
using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Shell.ContextMenu;
using Foldora.Shell.Registry;
using Foldora.Shell.RegistryPlan;

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

        case CliCommandKind.MenuReset:
            await ResetMenuAsync();
            return 0;

        case CliCommandKind.Skeleton:
            Console.WriteLine($"Command '{parsedCommand.Name}' is a skeleton and is not implemented yet.");
            return 0;

        case CliCommandKind.RegisterMenu:
            await RegisterMenuAsync(parsedCommand.DryRun, parsedCommand.CommandHostPath);
            return 0;

        case CliCommandKind.UnregisterMenu:
            await UnregisterMenuAsync();
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
                                  or IOException
                                  or System.Security.SecurityException)
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
  foldora menu reset --yes
  foldora register-menu [--dry-run] [--host-path "<absolute-path-to-Foldora.MenuHost.exe>"] [--cli-path "<legacy-dev-override>"]
  foldora unregister-menu
  foldora import-pack --path "<pack-path>"
  foldora list-packs
  foldora list-styles
  foldora settings

Implemented now:
  apply --folder --icon
  apply --folder --entry-id
  create --target --entry-id
  clear --folder
  menu list
  menu add --icon [--name] [--folder-name]
  menu remove --entry-id
  menu reset --yes
  register-menu [--dry-run] [--host-path] [--cli-path]
  unregister-menu

The --style flow, pack import, Explorer restart, and icon cache reset are not implemented in this step.
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

static ExplorerMenuRegistrationService CreateRegistrationService()
{
    var paths = FoldoraDataPaths.CreateDefault();
    var storage = new FoldoraSettingsStorage(paths);
    var writer = new ExplorerMenuRegistryWriter(new WindowsRegistryAccess());
    return new ExplorerMenuRegistrationService(storage, new ExplorerMenuRegistryPlanBuilder(), writer);
}

static async Task RegisterMenuAsync(bool dryRun, string? commandHostPath)
{
    var resolvedHostPath = string.IsNullOrWhiteSpace(commandHostPath)
        ? ResolveDefaultCommandHostPath()
        : commandHostPath;

    var result = await CreateRegistrationService().RegisterAsync(resolvedHostPath, dryRun);
    Console.WriteLine(result.Message);
    Console.WriteLine($"ExplorerIntegrationEnabled: {result.ExplorerIntegrationEnabled}");

    if (dryRun)
    {
        PrintPlans(result.Plans);
    }
}

static string ResolveDefaultCommandHostPath()
{
    var processPath = Environment.ProcessPath
        ?? throw new InvalidOperationException("Current CLI executable path could not be resolved.");
    var processDirectory = Path.GetDirectoryName(processPath)
        ?? throw new InvalidOperationException("Current CLI executable directory could not be resolved.");

    var installedHostPath = Path.Combine(processDirectory, "Foldora.MenuHost.exe");
    if (File.Exists(installedHostPath))
    {
        return installedHostPath;
    }

    var targetFrameworkDirectory = new DirectoryInfo(processDirectory);
    var configurationDirectory = targetFrameworkDirectory.Parent;
    if (configurationDirectory is not null)
    {
        var devHostPath = Path.GetFullPath(Path.Combine(
            processDirectory,
            "..",
            "..",
            "..",
            "..",
            "Foldora.MenuHost",
            "bin",
            configurationDirectory.Name,
            "net10.0-windows",
            "Foldora.MenuHost.exe"));

        if (File.Exists(devHostPath))
        {
            return devHostPath;
        }
    }

    return installedHostPath;
}

static async Task UnregisterMenuAsync()
{
    var result = await CreateRegistrationService().UnregisterAsync();
    Console.WriteLine(result.Message);
    Console.WriteLine($"ExplorerIntegrationEnabled: {result.ExplorerIntegrationEnabled}");
}

static async Task ResetMenuAsync()
{
    var result = await CreateRegistrationService().ResetMenuAsync();
    Console.WriteLine(result.Message);
    Console.WriteLine($"ExplorerIntegrationEnabled: {result.ExplorerIntegrationEnabled}");
}

static void PrintPlans(IEnumerable<ExplorerMenuRegistryPlan> plans)
{
    foreach (var plan in plans)
    {
        Console.WriteLine($"Plan: {plan.TargetKind}");

        foreach (var operation in plan.DeleteOperations)
        {
            Console.WriteLine($"  DELETE {operation.Hive}\\{operation.KeyPath}");
        }

        foreach (var operation in plan.KeyOperations)
        {
            Console.WriteLine($"  CREATE {operation.Hive}\\{operation.KeyPath}");
        }

        foreach (var operation in plan.ValueOperations)
        {
            var valueName = string.IsNullOrEmpty(operation.ValueName) ? "(Default)" : operation.ValueName;
            Console.WriteLine($"  SET {operation.Hive}\\{operation.KeyPath} [{valueName}] = {operation.ValueData}");
        }
    }
}
