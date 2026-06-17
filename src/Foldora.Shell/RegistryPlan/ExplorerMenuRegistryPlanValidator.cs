namespace Foldora.Shell.RegistryPlan;

/// <summary>
/// Проверяет, что registry plan не выходит за Foldora-owned HKCU roots.
/// </summary>
public sealed class ExplorerMenuRegistryPlanValidator
{
    public ExplorerMenuRegistryPlanValidationResult Validate(ExplorerMenuRegistryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var errors = new List<string>();

        foreach (var operation in plan.DeleteOperations)
        {
            ValidateOperation(operation.Hive, operation.KeyPath, "delete", errors);
        }

        foreach (var operation in plan.KeyOperations)
        {
            ValidateOperation(operation.Hive, operation.KeyPath, "create key", errors);
        }

        foreach (var operation in plan.ValueOperations)
        {
            ValidateOperation(operation.Hive, operation.KeyPath, "set value", errors);
        }

        return errors.Count == 0
            ? ExplorerMenuRegistryPlanValidationResult.Success
            : new ExplorerMenuRegistryPlanValidationResult(errors);
    }

    private static void ValidateOperation(
        ExplorerMenuRegistryHive hive,
        string keyPath,
        string operationKind,
        ICollection<string> errors)
    {
        if (hive != ExplorerMenuRegistryHive.CurrentUser)
        {
            errors.Add($"Registry plan contains non-HKCU {operationKind} operation: {hive}\\{keyPath}");
        }

        if (!ExplorerMenuRegistryPaths.IsInsideOwnedRoot(keyPath))
        {
            errors.Add($"Registry plan contains {operationKind} operation outside Foldora-owned roots: {keyPath}");
        }
    }
}
