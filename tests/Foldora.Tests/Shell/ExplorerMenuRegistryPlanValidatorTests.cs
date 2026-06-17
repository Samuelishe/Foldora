using Foldora.Shell.RegistryPlan;

namespace Foldora.Tests.Shell;

public sealed class ExplorerMenuRegistryPlanValidatorTests
{
    [Fact]
    public void Validate_RejectsOperationOutsideFoldoraOwnedRoot()
    {
        var plan = new ExplorerMenuRegistryPlan(
            ExplorerMenuTargetKind.Directory,
            [],
            [new ExplorerMenuRegistryKeyOperation(ExplorerMenuRegistryHive.CurrentUser, @"Software\Classes\Directory\shell\OtherApp")],
            []);

        var result = new ExplorerMenuRegistryPlanValidator().Validate(plan);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("outside Foldora-owned roots"));
    }

    [Fact]
    public void Validate_RejectsLocalMachineOperation()
    {
        var plan = new ExplorerMenuRegistryPlan(
            ExplorerMenuTargetKind.Directory,
            [],
            [new ExplorerMenuRegistryKeyOperation(ExplorerMenuRegistryHive.LocalMachine, ExplorerMenuRegistryPaths.DirectoryRoot)],
            []);

        var result = new ExplorerMenuRegistryPlanValidator().Validate(plan);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("non-HKCU"));
    }

    [Fact]
    public void Validate_AcceptsOperationsUnderBothOwnedRoots()
    {
        var plan = new ExplorerMenuRegistryPlan(
            ExplorerMenuTargetKind.Directory,
            [new ExplorerMenuRegistryDeleteOperation(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryRoot)],
            [new ExplorerMenuRegistryKeyOperation(ExplorerMenuRegistryHive.CurrentUser, ExplorerMenuRegistryPaths.DirectoryBackgroundRoot)],
            [new ExplorerMenuRegistryValueOperation(ExplorerMenuRegistryHive.CurrentUser, $@"{ExplorerMenuRegistryPaths.DirectoryRoot}\shell", "MUIVerb", "Foldora")]);

        var result = new ExplorerMenuRegistryPlanValidator().Validate(plan);

        Assert.True(result.IsValid);
    }
}
