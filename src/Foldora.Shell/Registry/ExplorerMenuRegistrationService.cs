using Foldora.Core.Menu;
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
        string commandHostPath,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        ValidateCommandHostPath(commandHostPath);

        var settings = await settingsStorage.LoadAsync(cancellationToken);
        var plans = BuildPlans(commandHostPath, settings);

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
        var plans = BuildDeletePlans(settings.CreateFolderMenu.Title);

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

    public Task<ExplorerMenuRegistrationResult> ResetMenuAsync(CancellationToken cancellationToken = default)
    {
        return ResetMenuAsync(FolderMenuSettings.CreateDefault(), cancellationToken);
    }

    public async Task<ExplorerMenuRegistrationResult> ResetMenuAsync(
        FolderMenuSettings defaultMenuSettings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(defaultMenuSettings);

        var settings = await settingsStorage.LoadAsync(cancellationToken);
        var plans = BuildDeletePlans(defaultMenuSettings.Title);

        foreach (var plan in plans)
        {
            writer.Apply(plan);
        }

        var updatedSettings = settings with
        {
            ExplorerIntegrationEnabled = false,
            CreateFolderMenu = defaultMenuSettings
        };
        await SaveSettingsAfterRegistrySuccessAsync(updatedSettings, cancellationToken);

        return new ExplorerMenuRegistrationResult(
            dryRun: false,
            applied: true,
            explorerIntegrationEnabled: false,
            message: "Foldora user menu was reset to the empty default.",
            plans);
    }

    private ExplorerMenuRegistryPlan[] BuildPlans(string commandHostPath, FoldoraSettings settings)
    {
        return
        [
            planBuilder.Build(commandHostPath, settings.CreateFolderMenu, ExplorerMenuTargetKind.Directory),
            planBuilder.Build(commandHostPath, settings.CreateFolderMenu, ExplorerMenuTargetKind.DirectoryBackground)
        ];
    }

    private ExplorerMenuRegistryPlan[] BuildDeletePlans(string title)
    {
        var emptyMenu = new FolderMenuSettings { Title = title };

        return
        [
            planBuilder.Build("C:\\Foldora\\Foldora.MenuHost.exe", emptyMenu, ExplorerMenuTargetKind.Directory),
            planBuilder.Build("C:\\Foldora\\Foldora.MenuHost.exe", emptyMenu, ExplorerMenuTargetKind.DirectoryBackground)
        ];
    }

    private static void ValidateCommandHostPath(string commandHostPath)
    {
        if (string.IsNullOrWhiteSpace(commandHostPath))
        {
            throw new InvalidOperationException("Explorer command host path is required.");
        }

        if (!Path.IsPathFullyQualified(commandHostPath))
        {
            throw new InvalidOperationException("Explorer command host path must be absolute.");
        }

        if (!File.Exists(commandHostPath))
        {
            throw new FileNotFoundException($"Explorer command host was not found: {commandHostPath}", commandHostPath);
        }

        if (!string.Equals(Path.GetExtension(commandHostPath), ".exe", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Explorer command host path must point to an .exe file.");
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
}
