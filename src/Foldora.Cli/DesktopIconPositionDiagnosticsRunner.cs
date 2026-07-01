using Foldora.Shell.Desktop;

namespace Foldora.Cli;

/// <summary>
/// Runner diagnostic-команды для ручной проверки reposition existing desktop icon.
/// </summary>
public sealed class DesktopIconPositionDiagnosticsRunner
{
    private readonly IDesktopIconPositioningService positioningService;

    public DesktopIconPositionDiagnosticsRunner(IDesktopIconPositioningService positioningService)
    {
        ArgumentNullException.ThrowIfNull(positioningService);
        this.positioningService = positioningService;
    }

    public Task<int> RunAsync(
        string itemName,
        int x,
        int y,
        DesktopIconCoordinateSpace coordinateSpace,
        TextWriter output,
        TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        var result = positioningService.TryPositionByName(itemName, x, y, coordinateSpace);
        if (result.Success)
        {
            output.WriteLine(result.Message);
            output.WriteLine("This was a diagnostic/prototype operation. It is not wired into Explorer menu create flow.");
            return Task.FromResult(0);
        }

        error.WriteLine($"Desktop icon positioning failed: {result.Message}");
        if (result.HResult is not null)
        {
            error.WriteLine($"HRESULT: 0x{result.HResult.Value:X8}");
        }

        return Task.FromResult(1);
    }
}
