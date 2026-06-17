using Foldora.Core.Settings;
using Foldora.Shell.RegistryPlan;

namespace Foldora.Shell.Registry;

/// <summary>
/// Оркестрирует register-menu, dry-run и unregister-menu поверх validated registry plan.
/// </summary>
public sealed class ExplorerMenuRegistrationService
{
    private readonly FoldoraSettingsStorage settingsStorage;
    private readonly ExplorerMenuRegistryPlanBuilder planBuilder;
    private readonly ExplorerMenuRegistryWriter writer;

    public ExplorerMenuRegistrationService(
        FoldoraSettingsStorage settingsStorage,
        ExplorerMenuRegistryPlanBuilder planBuilder,
        ExplorerMenuRegistryWriter writer)
    {
        this.settingsStorage = settingsStorage ?? throw new ArgumentNullException(nameof(settingsStorage));
        this.planBuilder = planBuilder ?? throw new ArgumentNullException(nameof(planBuilder));
        this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public async Task<ExplorerMenuRegistrationResult> RegisterAsync(
        string cliExecutablePath,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        ValidateCliExecutablePath(cliExecutablePath);

        var settings = await settingsStorage.LoadAsync(cancellationToken);
        var plans = BuildPlans(cliExecutablePath, settings);

        if (dryRun)
        {
            return new ExplorerMenuRegistrationResult(
                dryRun: true,
                applied: false,
                explorerIntegrationEnabled: settings.ExplorerIntegrationEnabled,
                message: "Dry run completed. Registry was not changed.",
                plans);
        }

        foreach (var plan in plans)
        {
            writer.Apply(plan);
        }

        var hasEnabledEntries = settings.CreateFolderMenu.Entries.Any(entry => entry.IsEnabled);
        var updatedSettings = settings with { ExplorerIntegrationEnabled = hasEnabledEntries };
        await SaveSettingsAfterRegistrySuccessAsync(updatedSettings, cancellationToken);

        var message = hasEnabledEntries
            ? "Explorer legacy context menu was registered."
            : "No enabled menu entries. Foldora menu was removed and not registered.";

        return new ExplorerMenuRegistrationResult(
            dryRun: false,
            applied: true,
            explorerIntegrationEnabled: updatedSettings.ExplorerIntegrationEnabled,
            message: message,
            plans);
    }

    public async Task<ExplorerMenuRegistrationResult> UnregisterAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsStorage.LoadAsync(cancellationToken);
        var emptySettings = FolderMenuSettingsFactory.CreateEmptyLike(settings);
        var plans = BuildPlans("C:\\Foldora\\Foldora.Cli.exe", emptySettings);

        foreach (var plan in plans)
        {
            writer.Apply(plan);
        }

        var updatedSettings = settings with { ExplorerIntegrationEnabled = false };
        await SaveSettingsAfterRegistrySuccessAsync(updatedSettings, cancellationToken);

        return new ExplorerMenuRegistrationResult(
            dryRun: false,
            applied: true,
            explorerIntegrationEnabled: false,
            message: "Explorer legacy context menu was unregistered.",
            plans);
    }

    private ExplorerMenuRegistryPlan[] BuildPlans(string cliExecutablePath, FoldoraSettings settings)
    {
        return
        [
            planBuilder.Build(cliExecutablePath, settings.CreateFolderMenu, ExplorerMenuTargetKind.Directory),
            planBuilder.Build(cliExecutablePath, settings.CreateFolderMenu, ExplorerMenuTargetKind.DirectoryBackground)
        ];
    }

    private static void ValidateCliExecutablePath(string cliExecutablePath)
    {
        if (string.IsNullOrWhiteSpace(cliExecutablePath))
        {
            throw new InvalidOperationException("CLI executable path is required.");
        }

        if (!Path.IsPathFullyQualified(cliExecutablePath))
        {
            throw new InvalidOperationException("CLI executable path must be absolute.");
        }

        if (!File.Exists(cliExecutablePath))
        {
            throw new FileNotFoundException($"CLI executable was not found: {cliExecutablePath}", cliExecutablePath);
        }

        if (!string.Equals(Path.GetExtension(cliExecutablePath), ".exe", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("CLI executable path must point to an .exe file.");
        }
    }

    private async Task SaveSettingsAfterRegistrySuccessAsync(
        FoldoraSettings settings,
        CancellationToken cancellationToken)
    {
        try
        {
            await settingsStorage.SaveAsync(settings, cancellationToken);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException("Registry menu was updated, but settings could not be saved.", exception);
        }
    }

    private static class FolderMenuSettingsFactory
    {
        public static FoldoraSettings CreateEmptyLike(FoldoraSettings settings)
        {
            return settings with { CreateFolderMenu = new Foldora.Core.Menu.FolderMenuSettings { Title = settings.CreateFolderMenu.Title } };
        }
    }
}
