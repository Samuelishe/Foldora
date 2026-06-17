namespace Foldora.Shell.RegistryPlan;

/// <summary>
/// Результат проверки безопасности registry plan.
/// </summary>
public sealed class ExplorerMenuRegistryPlanValidationResult
{
    public ExplorerMenuRegistryPlanValidationResult(IEnumerable<string> errors)
    {
        Errors = errors.ToArray();
    }

    public IReadOnlyList<string> Errors { get; }

    public bool IsValid => Errors.Count == 0;

    public static ExplorerMenuRegistryPlanValidationResult Success { get; } = new([]);
}
