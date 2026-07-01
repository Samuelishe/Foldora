namespace Foldora.Shell.Desktop;

/// <summary>
/// Diagnostic/prototype API для позиционирования существующего desktop item.
/// </summary>
public interface IDesktopIconPositioningService
{
    DesktopIconPositioningResult TryPositionByName(
        string itemName,
        int x,
        int y,
        DesktopIconCoordinateSpace coordinateSpace);
}
