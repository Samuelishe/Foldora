using Foldora.Shell.RegistryPlan;

namespace Foldora.Shell.Registry;

/// <summary>
/// Применяет только validated registry plan через абстракцию registry access.
/// </summary>
public sealed class ExplorerMenuRegistryWriter
{
    private readonly IRegistryAccess registryAccess;
    private readonly ExplorerMenuRegistryPlanValidator validator;

    public ExplorerMenuRegistryWriter(
        IRegistryAccess registryAccess,
        ExplorerMenuRegistryPlanValidator? validator = null)
    {
        this.registryAccess = registryAccess ?? throw new ArgumentNullException(nameof(registryAccess));
        this.validator = validator ?? new ExplorerMenuRegistryPlanValidator();
    }

    public void Apply(ExplorerMenuRegistryPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var validation = validator.Validate(plan);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException($"Registry plan validation failed: {validation.Errors[0]}");
        }

        foreach (var operation in plan.DeleteOperations)
        {
            registryAccess.DeleteTreeIfExists(operation.Hive, operation.KeyPath);
        }

        foreach (var operation in plan.KeyOperations)
        {
            registryAccess.CreateKey(operation.Hive, operation.KeyPath);
        }

        foreach (var operation in plan.ValueOperations)
        {
            registryAccess.SetStringValue(
                operation.Hive,
                operation.KeyPath,
                operation.ValueName,
                operation.ValueData);
        }
    }
}
