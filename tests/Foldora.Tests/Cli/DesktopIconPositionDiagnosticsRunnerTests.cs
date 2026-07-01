using Foldora.Cli;
using Foldora.Shell.Desktop;

namespace Foldora.Tests.Cli;

public sealed class DesktopIconPositionDiagnosticsRunnerTests
{
    [Fact]
    public async Task RunAsync_ReturnsZeroWhenPositioningSucceeds()
    {
        var service = new FakeDesktopIconPositioningService(
            DesktopIconPositioningResult.Succeeded("Moved."));
        using var output = new StringWriter();
        using var error = new StringWriter();
        var runner = new DesktopIconPositionDiagnosticsRunner(service);

        var exitCode = await runner.RunAsync(
            "Test Folder",
            100,
            200,
            DesktopIconCoordinateSpace.View,
            output,
            error);

        Assert.Equal(0, exitCode);
        Assert.Equal("Test Folder", service.ItemName);
        Assert.Equal(100, service.X);
        Assert.Equal(200, service.Y);
        Assert.Equal(DesktopIconCoordinateSpace.View, service.CoordinateSpace);
        Assert.Contains("Moved.", output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Fact]
    public async Task RunAsync_ReturnsOneWhenPositioningFails()
    {
        var service = new FakeDesktopIconPositioningService(
            DesktopIconPositioningResult.Failed("Item was not found.", unchecked((int)0x80004005)));
        using var output = new StringWriter();
        using var error = new StringWriter();
        var runner = new DesktopIconPositionDiagnosticsRunner(service);

        var exitCode = await runner.RunAsync(
            "Missing",
            10,
            20,
            DesktopIconCoordinateSpace.Screen,
            output,
            error);

        Assert.Equal(1, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("Item was not found.", error.ToString());
        Assert.Contains("0x80004005", error.ToString());
    }

    private sealed class FakeDesktopIconPositioningService : IDesktopIconPositioningService
    {
        private readonly DesktopIconPositioningResult result;

        public FakeDesktopIconPositioningService(DesktopIconPositioningResult result)
        {
            this.result = result;
        }

        public string? ItemName { get; private set; }

        public int X { get; private set; }

        public int Y { get; private set; }

        public DesktopIconCoordinateSpace CoordinateSpace { get; private set; }

        public DesktopIconPositioningResult TryPositionByName(
            string itemName,
            int x,
            int y,
            DesktopIconCoordinateSpace coordinateSpace)
        {
            ItemName = itemName;
            X = x;
            Y = y;
            CoordinateSpace = coordinateSpace;
            return result;
        }
    }
}
