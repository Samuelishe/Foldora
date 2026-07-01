namespace Foldora.Shell.Desktop;

/// <summary>
/// Результат prototype-попытки позиционировать desktop icon.
/// </summary>
public sealed record DesktopIconPositioningResult(
    bool Success,
    string Message,
    int? HResult = null)
{
    public static DesktopIconPositioningResult Succeeded(string message)
    {
        return new DesktopIconPositioningResult(true, message);
    }

    public static DesktopIconPositioningResult Failed(string message, int? hResult = null)
    {
        return new DesktopIconPositioningResult(false, message, hResult);
    }
}
