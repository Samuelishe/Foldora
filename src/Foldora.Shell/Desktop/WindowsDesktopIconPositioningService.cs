using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace Foldora.Shell.Desktop;

/// <summary>
/// Windows-only prototype для попытки переместить существующий desktop icon через Explorer folder view.
/// </summary>
public sealed class WindowsDesktopIconPositioningService : IDesktopIconPositioningService
{
    private const uint SvgioAllView = 0x00000002;
    private const uint ShgdnNormal = 0x00000000;
    private const uint SvsiSelect = 0x00000001;
    private const uint SvsiEnsureVisible = 0x00000008;
    private const uint SvsiFocused = 0x00000010;
    private const uint SvsiPositionItem = 0x00000080;

    private static readonly Guid ClsidShellWindows = new("9BA05972-F6A8-11CF-A442-00A0C90A8F39");
    private static readonly Guid SidSTopLevelBrowser = new("4C96BE40-915C-11CF-99D3-00AA004AE837");
    private static readonly Guid IidIShellBrowser = new("000214E2-0000-0000-C000-000000000046");
    private static readonly Guid IidIShellFolder = new("000214E6-0000-0000-C000-000000000046");

    public DesktopIconPositioningResult TryPositionByName(
        string itemName,
        int x,
        int y,
        DesktopIconCoordinateSpace coordinateSpace)
    {
        if (!OperatingSystem.IsWindows())
        {
            return DesktopIconPositioningResult.Failed("Desktop icon positioning prototype is Windows-only.");
        }

        if (string.IsNullOrWhiteSpace(itemName))
        {
            return DesktopIconPositioningResult.Failed("Desktop item name is required.");
        }

        try
        {
            var point = new NativePoint(x, y);
            if (coordinateSpace == DesktopIconCoordinateSpace.Screen && !TryConvertScreenToDesktopView(ref point))
            {
                return DesktopIconPositioningResult.Failed("Desktop list view window was not found for screen-coordinate conversion.");
            }

            var folderView = GetDesktopFolderView();
            var item = FindItem(folderView, itemName.Trim());
            if (item.Pidl == IntPtr.Zero)
            {
                return DesktopIconPositioningResult.Failed($"Desktop item was not found: {itemName}");
            }

            try
            {
                var pidls = new[] { item.Pidl };
                var points = new[] { point };
                var flags = SvsiSelect | SvsiFocused | SvsiEnsureVisible | SvsiPositionItem;
                var result = folderView.SelectAndPositionItems(1, pidls, points, flags);
                if (result != 0)
                {
                    return DesktopIconPositioningResult.Failed(
                        $"Explorer rejected desktop icon positioning. HRESULT: 0x{result:X8}.",
                        result);
                }

                return DesktopIconPositioningResult.Succeeded(
                    $"Desktop item '{item.DisplayName}' was positioned at view coordinates ({point.X}, {point.Y}).");
            }
            finally
            {
                Marshal.FreeCoTaskMem(item.Pidl);
            }
        }
        catch (COMException exception)
        {
            return DesktopIconPositioningResult.Failed(
                $"Shell COM positioning failed. HRESULT: 0x{exception.HResult:X8}. {exception.Message}",
                exception.HResult);
        }
        catch (Exception exception) when (exception is InvalidOperationException
                                         or ArgumentException
                                         or UnauthorizedAccessException
                                         or System.Security.SecurityException)
        {
            return DesktopIconPositioningResult.Failed(exception.Message);
        }
    }

    [SupportedOSPlatform("windows")]
    private static IFolderView GetDesktopFolderView()
    {
        var shellWindowsType = Type.GetTypeFromCLSID(ClsidShellWindows, throwOnError: true)
            ?? throw new InvalidOperationException("ShellWindows COM type was not found.");
        var shellWindows = Activator.CreateInstance(shellWindowsType)
            ?? throw new InvalidOperationException("ShellWindows COM object could not be created.");

        try
        {
            const int swcDesktop = 8;
            const int swfoNeedDispatch = 1;
            object location = 0;
            object root = 0;
            dynamic windows = shellWindows;
            var dispatch = windows.FindWindowSW(ref location, ref root, swcDesktop, out int _, swfoNeedDispatch);
            if (dispatch is null)
            {
                throw new InvalidOperationException("Explorer desktop dispatch object was not found.");
            }

            var dispatchUnknown = Marshal.GetIUnknownForObject(dispatch);
            try
            {
                var serviceProvider = (NativeServiceProvider)Marshal.GetObjectForIUnknown(dispatchUnknown);
                var serviceId = SidSTopLevelBrowser;
                var browserId = IidIShellBrowser;
                var queryResult = serviceProvider.QueryService(ref serviceId, ref browserId, out var browserPointer);
                if (queryResult != 0 || browserPointer == IntPtr.Zero)
                {
                    throw new COMException("Explorer top-level browser service was not found.", queryResult);
                }

                try
                {
                    var shellBrowser = (IShellBrowser)Marshal.GetObjectForIUnknown(browserPointer);
                    var viewResult = shellBrowser.QueryActiveShellView(out var shellView);
                    if (viewResult != 0 || shellView is null)
                    {
                        throw new COMException("Explorer active shell view was not found.", viewResult);
                    }

                    return (IFolderView)shellView;
                }
                finally
                {
                    Marshal.Release(browserPointer);
                }
            }
            finally
            {
                Marshal.Release(dispatchUnknown);
            }
        }
        finally
        {
            if (Marshal.IsComObject(shellWindows))
            {
                Marshal.FinalReleaseComObject(shellWindows);
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private static DesktopItem FindItem(IFolderView folderView, string itemName)
    {
        var folderId = IidIShellFolder;
        var folderResult = folderView.GetFolder(ref folderId, out var folderPointer);
        if (folderResult != 0 || folderPointer == IntPtr.Zero)
        {
            throw new COMException("Desktop IShellFolder was not found.", folderResult);
        }

        try
        {
            var shellFolder = (IShellFolder)Marshal.GetObjectForIUnknown(folderPointer);
            var countResult = folderView.ItemCount(SvgioAllView, out var itemCount);
            if (countResult != 0)
            {
                throw new COMException("Desktop item count could not be read.", countResult);
            }

            for (var index = 0; index < itemCount; index++)
            {
                var itemResult = folderView.Item(index, out var pidl);
                if (itemResult != 0 || pidl == IntPtr.Zero)
                {
                    continue;
                }

                var displayName = GetDisplayName(shellFolder, pidl);
                if (string.Equals(displayName, itemName, StringComparison.CurrentCultureIgnoreCase)
                    || string.Equals(displayName, itemName, StringComparison.OrdinalIgnoreCase))
                {
                    return new DesktopItem(pidl, displayName);
                }

                Marshal.FreeCoTaskMem(pidl);
            }

            return new DesktopItem(IntPtr.Zero, string.Empty);
        }
        finally
        {
            Marshal.Release(folderPointer);
        }
    }

    private static string GetDisplayName(IShellFolder shellFolder, IntPtr pidl)
    {
        var result = shellFolder.GetDisplayNameOf(pidl, ShgdnNormal, out var strret);
        if (result != 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(260);
        var bufferResult = StrRetToBuf(ref strret, pidl, builder, (uint)builder.Capacity);
        return bufferResult == 0 ? builder.ToString() : string.Empty;
    }

    [SupportedOSPlatform("windows")]
    private static bool TryConvertScreenToDesktopView(ref NativePoint point)
    {
        var desktopListView = FindDesktopListViewWindow();
        return desktopListView != IntPtr.Zero && ScreenToClient(desktopListView, ref point);
    }

    [SupportedOSPlatform("windows")]
    private static IntPtr FindDesktopListViewWindow()
    {
        var progman = FindWindow("Progman", "Program Manager");
        var shellView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
        if (shellView != IntPtr.Zero)
        {
            return FindWindowEx(shellView, IntPtr.Zero, "SysListView32", "FolderView");
        }

        var result = IntPtr.Zero;
        EnumWindows((window, _) =>
        {
            var workerShellView = FindWindowEx(window, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (workerShellView == IntPtr.Zero)
            {
                return true;
            }

            result = FindWindowEx(workerShellView, IntPtr.Zero, "SysListView32", "FolderView");
            return result == IntPtr.Zero;
        }, IntPtr.Zero);

        return result;
    }

    [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern int StrRetToBuf(
        ref NativeStrRet pstr,
        IntPtr pidl,
        StringBuilder pszBuf,
        uint cchBuf);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr FindWindowEx(
        IntPtr hwndParent,
        IntPtr hwndChildAfter,
        string lpszClass,
        string? lpszWindow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ScreenToClient(IntPtr hWnd, ref NativePoint lpPoint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private readonly record struct DesktopItem(IntPtr Pidl, string DisplayName);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public NativePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential, Size = 520)]
    private struct NativeStrRet
    {
        public uint UType;
    }

    [ComImport]
    [Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface NativeServiceProvider
    {
        [PreserveSig]
        int QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject);
    }

    [ComImport]
    [Guid("000214E2-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellBrowser
    {
        [PreserveSig]
        int GetWindow(out IntPtr phwnd);

        [PreserveSig]
        int ContextSensitiveHelp(bool fEnterMode);

        [PreserveSig]
        int InsertMenusSB(IntPtr hmenuShared, IntPtr lpMenuWidths);

        [PreserveSig]
        int SetMenuSB(IntPtr hmenuShared, IntPtr holemenuRes, IntPtr hwndActiveObject);

        [PreserveSig]
        int RemoveMenusSB(IntPtr hmenuShared);

        [PreserveSig]
        int SetStatusTextSB(IntPtr pszStatusText);

        [PreserveSig]
        int EnableModelessSB(bool fEnable);

        [PreserveSig]
        int TranslateAcceleratorSB(IntPtr pmsg, short wID);

        [PreserveSig]
        int BrowseObject(IntPtr pidl, uint wFlags);

        [PreserveSig]
        int GetViewStateStream(uint grfMode, out IntPtr ppStrm);

        [PreserveSig]
        int GetControlWindow(uint id, out IntPtr phwnd);

        [PreserveSig]
        int SendControlMsg(uint id, uint uMsg, UIntPtr wParam, IntPtr lParam, out IntPtr pret);

        [PreserveSig]
        int QueryActiveShellView([MarshalAs(UnmanagedType.Interface)] out IShellView ppshv);

        [PreserveSig]
        int OnViewWindowActive(IShellView pshv);

        [PreserveSig]
        int SetToolbarItems(IntPtr lpButtons, uint nButtons, uint uFlags);
    }

    [ComImport]
    [Guid("000214E3-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellView
    {
    }

    [ComImport]
    [Guid("CDE725B0-CCC9-4519-917E-325D72FAB4CE")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFolderView
    {
        [PreserveSig]
        int GetCurrentViewMode(out uint pViewMode);

        [PreserveSig]
        int SetCurrentViewMode(uint viewMode);

        [PreserveSig]
        int GetFolder(ref Guid riid, out IntPtr ppv);

        [PreserveSig]
        int Item(int iItemIndex, out IntPtr ppidl);

        [PreserveSig]
        int ItemCount(uint uFlags, out int pcItems);

        [PreserveSig]
        int Items(uint uFlags, ref Guid riid, out IntPtr ppv);

        [PreserveSig]
        int GetSelectionMarkedItem(out int piItem);

        [PreserveSig]
        int GetFocusedItem(out int piItem);

        [PreserveSig]
        int GetItemPosition(IntPtr pidl, out NativePoint ppt);

        [PreserveSig]
        int GetSpacing(out NativePoint ppt);

        [PreserveSig]
        int GetDefaultSpacing(out NativePoint ppt);

        [PreserveSig]
        int GetAutoArrange();

        [PreserveSig]
        int SelectItem(int iItem, uint dwFlags);

        [PreserveSig]
        int SelectAndPositionItems(
            uint cidl,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]
            IntPtr[] apidl,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]
            NativePoint[] apt,
            uint dwFlags);
    }

    [ComImport]
    [Guid("000214E6-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellFolder
    {
        [PreserveSig]
        int ParseDisplayName(IntPtr hwnd, IntPtr pbc, IntPtr pszDisplayName, IntPtr pchEaten, out IntPtr ppidl, IntPtr pdwAttributes);

        [PreserveSig]
        int EnumObjects(IntPtr hwnd, uint grfFlags, out IntPtr ppenumIDList);

        [PreserveSig]
        int BindToObject(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);

        [PreserveSig]
        int BindToStorage(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);

        [PreserveSig]
        int CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);

        [PreserveSig]
        int CreateViewObject(IntPtr hwndOwner, ref Guid riid, out IntPtr ppv);

        [PreserveSig]
        int GetAttributesOf(uint cidl, IntPtr apidl, ref uint rgfInOut);

        [PreserveSig]
        int GetUIObjectOf(IntPtr hwndOwner, uint cidl, IntPtr apidl, ref Guid riid, IntPtr rgfReserved, out IntPtr ppv);

        [PreserveSig]
        int GetDisplayNameOf(IntPtr pidl, uint uFlags, out NativeStrRet pName);

        [PreserveSig]
        int SetNameOf(IntPtr hwnd, IntPtr pidl, IntPtr pszName, uint uFlags, out IntPtr ppidlOut);
    }
}
