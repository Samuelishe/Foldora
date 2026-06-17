using Foldora.Shell.RegistryPlan;

namespace Foldora.Shell.Registry;

/// <summary>
/// Результат регистрации, dry-run или удаления legacy context menu.
/// </summary>
public sealed class ExplorerMenuRegistrationResult
{
    public ExplorerMenuRegistrationResult(
        bool dryRun,
        bool applied,
        bool explorerIntegrationEnabled,
        string message,
        IEnumerable<ExplorerMenuRegistryPlan> plans)
    {
        DryRun = dryRun;
        Applied = applied;
        ExplorerIntegrationEnabled = explorerIntegrationEnabled;
        Message = message;
        Plans = plans.ToArray();
    }

    public bool DryRun { get; }

    public bool Applied { get; }

    public bool ExplorerIntegrationEnabled { get; }

    public string Message { get; }

    public IReadOnlyList<ExplorerMenuRegistryPlan> Plans { get; }
}
