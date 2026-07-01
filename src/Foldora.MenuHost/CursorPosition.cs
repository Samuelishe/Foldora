using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Foldora.MenuHost;

internal readonly record struct CursorPosition(int X, int Y);

internal interface ICursorPositionProvider
{
    CursorPosition? TryGetCursorPosition();
}

internal sealed class WindowsCursorPositionProvider : ICursorPositionProvider
{
    public CursorPosition? TryGetCursorPosition()
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        return TryGetCursorPositionWindows(out var point)
            ? new CursorPosition(point.X, point.Y)
            : null;
    }

    [SupportedOSPlatform("windows")]
    private static bool TryGetCursorPositionWindows(out NativePoint point)
    {
        return GetCursorPos(out point);
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out NativePoint lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }
}
