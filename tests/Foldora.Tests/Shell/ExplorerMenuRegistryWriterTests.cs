using Foldora.Shell.Registry;
using Foldora.Shell.RegistryPlan;
using Foldora.Tests.Shell.Fakes;

namespace Foldora.Tests.Shell;

public sealed class ExplorerMenuRegistryWriterTests
{
    [Fact]
    public void Apply_RejectsInvalidPlanAndDoesNotTouchRegistry()
    {
        var registry = new FakeRegistryAccess();
        var writer = new ExplorerMenuRegistryWriter(registry);
        var plan = new ExplorerMenuRegistryPlan(
            ExplorerMenuTargetKind.Directory,
            [],
            [new ExplorerMenuRegistryKeyOperation(ExplorerMenuRegistryHive.CurrentUser, @"Software\Classes\Directory\shell\OtherApp")],
            []);

        Assert.Throws<InvalidOperationException>(() => writer.Apply(plan));
        Assert.Empty(registry.Calls);
    }

    [Fact]
    public void Apply_RejectsHklmPlanAndDoesNotTouchRegistry()
    {
        var registry = new FakeRegistryAccess();
        var writer = new ExplorerMenuRegistryWriter(registry);
        var plan = new ExplorerMenuRegistryPlan(
            ExplorerMenuTargetKind.Directory,
            [],
            [new ExplorerMenuRegistryKeyOperation(ExplorerMenuRegistryHive.LocalMachine, ExplorerMenuRegistryPaths.DirectoryRoot)],
            []);

        Assert.Throws<InvalidOperationException>(() => writer.Apply(plan));
        Assert.Empty(registry.Calls);
    }
}
