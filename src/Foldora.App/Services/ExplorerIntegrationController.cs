using System.IO;
using Foldora.App.ViewModels;
using Foldora.Core.Menu;
using Foldora.Shell.Registry;

namespace Foldora.App.Services;

/// <summary>
/// App-level controller для явных WPF-действий Explorer integration.
/// </summary>
public sealed class ExplorerIntegrationController
{
    private readonly FolderMenuDraftEditor draftEditor;
    private readonly ExplorerMenuRegistrationService registrationService;
    private readonly IExplorerCommandHostPathResolver commandHostPathResolver;
    private readonly ILocalizationService localizationService;

    public ExplorerIntegrationController(
        FolderMenuDraftEditor draftEditor,
        ExplorerMenuRegistrationService registrationService,
        IExplorerCommandHostPathResolver commandHostPathResolver,
        ILocalizationService? localizationService = null)
    {
        this.draftEditor = draftEditor ?? throw new ArgumentNullException(nameof(draftEditor));
        this.registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
        this.commandHostPathResolver = commandHostPathResolver ?? throw new ArgumentNullException(nameof(commandHostPathResolver));
        this.localizationService = localizationService ?? new InMemoryLocalizationService();
    }

    public bool ExplorerIntegrationEnabled => draftEditor.ExplorerIntegrationEnabled;

    public async Task<ExplorerIntegrationOperationResult> DryRunAsync(CancellationToken cancellationToken = default)
    {
        if (draftEditor.HasUnsavedChanges)
        {
            return BlockedByUnsavedChanges(draftEditor.ExplorerIntegrationEnabled);
        }

        try
        {
            var result = await registrationService.RegisterAsync(
                commandHostPathResolver.ResolveCommandHostPath(),
                dryRun: true,
                cancellationToken);

            return new ExplorerIntegrationOperationResult(
                true,
                L.PlanChecked,
                result.ExplorerIntegrationEnabled,
                BuildPlanSummary(result));
        }
        catch (Exception exception) when (IsUserFacingOperationException(exception))
        {
            return Failed(exception, draftEditor.ExplorerIntegrationEnabled);
        }
    }

    public async Task<ExplorerIntegrationOperationResult> RegisterAsync(CancellationToken cancellationToken = default)
    {
        if (draftEditor.HasUnsavedChanges)
        {
            return BlockedByUnsavedChanges(draftEditor.ExplorerIntegrationEnabled);
        }

        try
        {
            var result = await registrationService.RegisterAsync(
                commandHostPathResolver.ResolveCommandHostPath(),
                dryRun: false,
                cancellationToken);

            await draftEditor.LoadAsync(cancellationToken);

            var message = result.ExplorerIntegrationEnabled
                ? L.ExplorerMenuEnabled
                : L.ExplorerNoEntriesNotCreated;

            return new ExplorerIntegrationOperationResult(
                true,
                message,
                result.ExplorerIntegrationEnabled,
                BuildPlanSummary(result));
        }
        catch (Exception exception) when (IsUserFacingOperationException(exception))
        {
            return Failed(exception, draftEditor.ExplorerIntegrationEnabled);
        }
    }

    public async Task<ExplorerIntegrationOperationResult> RebuildAfterSaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await registrationService.RegisterAsync(
                commandHostPathResolver.ResolveCommandHostPath(),
                dryRun: false,
                cancellationToken);

            await draftEditor.LoadAsync(cancellationToken);

            var message = result.ExplorerIntegrationEnabled
                ? L.SettingsSavedExplorerUpdated
                : L.SettingsSavedNoEntriesExplorerDisabled;

            return new ExplorerIntegrationOperationResult(
                true,
                message,
                result.ExplorerIntegrationEnabled,
                BuildPlanSummary(result));
        }
        catch (Exception exception) when (IsUserFacingOperationException(exception))
        {
            return new ExplorerIntegrationOperationResult(
                false,
                L.SettingsSavedExplorerNotUpdated,
                draftEditor.ExplorerIntegrationEnabled,
                [string.Format(L.ExplorerIntegrationErrorFormat, exception.Message)]);
        }
    }

    public async Task<ExplorerIntegrationOperationResult> UnregisterAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var hadUnsavedChanges = draftEditor.HasUnsavedChanges;
            var result = await registrationService.UnregisterAsync(cancellationToken);
            if (!hadUnsavedChanges)
            {
                await draftEditor.LoadAsync(cancellationToken);
            }

            return new ExplorerIntegrationOperationResult(
                true,
                L.ExplorerMenuDisabled,
                result.ExplorerIntegrationEnabled,
                BuildPlanSummary(result));
        }
        catch (Exception exception) when (IsUserFacingOperationException(exception))
        {
            return Failed(exception, draftEditor.ExplorerIntegrationEnabled);
        }
    }

    public async Task<ExplorerIntegrationOperationResult> ResetMenuAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await registrationService.ResetMenuAsync(cancellationToken);
            await draftEditor.LoadAsync(cancellationToken);

            return new ExplorerIntegrationOperationResult(
                true,
                L.MenuReset,
                result.ExplorerIntegrationEnabled,
                BuildPlanSummary(result));
        }
        catch (Exception exception) when (IsUserFacingOperationException(exception))
        {
            return Failed(exception, draftEditor.ExplorerIntegrationEnabled);
        }
    }

    private ExplorerIntegrationOperationResult BlockedByUnsavedChanges(bool explorerIntegrationEnabled)
    {
        return new ExplorerIntegrationOperationResult(
            false,
            L.SaveUnsavedChangesFirst,
            explorerIntegrationEnabled);
    }

    private ExplorerIntegrationOperationResult Failed(Exception exception, bool explorerIntegrationEnabled)
    {
        return new ExplorerIntegrationOperationResult(
            false,
            string.Format(L.ExplorerIntegrationErrorFormat, exception.Message),
            explorerIntegrationEnabled);
    }

    private string[] BuildPlanSummary(ExplorerMenuRegistrationResult result)
    {
        var plans = result.Plans;
        var deleteCount = plans.Sum(plan => plan.DeleteOperations.Count);
        var createCount = plans.Sum(plan => plan.KeyOperations.Count);
        var setCount = plans.Sum(plan => plan.ValueOperations.Count);
        var roots = plans
            .SelectMany(plan => plan.DeleteOperations.Select(operation => operation.KeyPath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var commandExample = plans
            .SelectMany(plan => plan.ValueOperations)
            .FirstOrDefault(operation => operation.ValueName == string.Empty
                                         && operation.KeyPath.EndsWith(@"\command", StringComparison.OrdinalIgnoreCase));

        var summary = new List<string>
        {
            $"Plans: {plans.Count}. Operations: delete {deleteCount}, create {createCount}, set {setCount}."
        };

        foreach (var root in roots)
        {
            summary.Add($"Root: HKCU\\{root}");
        }

        if (commandExample is not null)
        {
            summary.Add($"Command example: {commandExample.ValueData}");
        }

        if (createCount == 0 && setCount == 0)
        {
            summary.Add(L.NoEnabledEntriesDetail);
        }

        return summary.ToArray();
    }

    private LocalizationResources L => localizationService.Resources;

    private static bool IsUserFacingOperationException(Exception exception)
    {
        return exception is InvalidOperationException
            or FileNotFoundException
            or DirectoryNotFoundException
            or UnauthorizedAccessException
            or IOException
            or ArgumentException
            or System.Security.SecurityException;
    }
}
