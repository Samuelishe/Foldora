using System.IO;
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
    private readonly IExplorerCliPathResolver cliPathResolver;

    public ExplorerIntegrationController(
        FolderMenuDraftEditor draftEditor,
        ExplorerMenuRegistrationService registrationService,
        IExplorerCliPathResolver cliPathResolver)
    {
        this.draftEditor = draftEditor ?? throw new ArgumentNullException(nameof(draftEditor));
        this.registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
        this.cliPathResolver = cliPathResolver ?? throw new ArgumentNullException(nameof(cliPathResolver));
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
                cliPathResolver.ResolveCliPath(),
                dryRun: true,
                cancellationToken);

            return new ExplorerIntegrationOperationResult(
                true,
                "План проверен.",
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
                cliPathResolver.ResolveCliPath(),
                dryRun: false,
                cancellationToken);

            await draftEditor.LoadAsync(cancellationToken);

            var message = result.ExplorerIntegrationEnabled
                ? "Меню Проводника включено."
                : "Нет включённых пунктов меню. Меню Проводника не создано.";

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
                "Меню Проводника отключено.",
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
                "Список пунктов меню сброшен.",
                result.ExplorerIntegrationEnabled,
                BuildPlanSummary(result));
        }
        catch (Exception exception) when (IsUserFacingOperationException(exception))
        {
            return Failed(exception, draftEditor.ExplorerIntegrationEnabled);
        }
    }

    private static ExplorerIntegrationOperationResult BlockedByUnsavedChanges(bool explorerIntegrationEnabled)
    {
        return new ExplorerIntegrationOperationResult(
            false,
            "Сначала сохраните изменения.",
            explorerIntegrationEnabled);
    }

    private static ExplorerIntegrationOperationResult Failed(Exception exception, bool explorerIntegrationEnabled)
    {
        return new ExplorerIntegrationOperationResult(
            false,
            $"Ошибка Explorer integration: {exception.Message}",
            explorerIntegrationEnabled);
    }

    private static string[] BuildPlanSummary(ExplorerMenuRegistrationResult result)
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
            summary.Add("Нет включённых пунктов меню. Меню не будет создано.");
        }

        return summary.ToArray();
    }

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
