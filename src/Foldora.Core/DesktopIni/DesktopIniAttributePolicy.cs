namespace Foldora.Core.DesktopIni;

/// <summary>
/// Описывает, какие файловые атрибуты Foldora выставляет папке и desktop.ini.
/// </summary>
public sealed record DesktopIniAttributePolicy(
    string Name,
    string DiagnosticFolderName,
    FileAttributes FolderAttributes,
    FileAttributes DesktopIniAttributes)
{
    public static DesktopIniAttributePolicy CompatibilitySystem { get; } = new(
        nameof(CompatibilitySystem),
        "CompatibilitySystem",
        FileAttributes.System,
        FileAttributes.Hidden | FileAttributes.System);

    public static DesktopIniAttributePolicy ReadOnlyFolderSystemDesktopIni { get; } = new(
        nameof(ReadOnlyFolderSystemDesktopIni),
        "ReadOnly SystemIni",
        FileAttributes.ReadOnly,
        FileAttributes.Hidden | FileAttributes.System);

    public static DesktopIniAttributePolicy ReadOnlyFolderHiddenDesktopIni { get; } = new(
        nameof(ReadOnlyFolderHiddenDesktopIni),
        "ReadOnly HiddenIni",
        FileAttributes.ReadOnly,
        FileAttributes.Hidden);

    public static DesktopIniAttributePolicy SystemFolderHiddenDesktopIni { get; } = new(
        nameof(SystemFolderHiddenDesktopIni),
        "System HiddenIni",
        FileAttributes.System,
        FileAttributes.Hidden);

    public static DesktopIniAttributePolicy Default => CompatibilitySystem;

    public static IReadOnlyList<DesktopIniAttributePolicy> Supported { get; } =
    [
        CompatibilitySystem,
        ReadOnlyFolderSystemDesktopIni,
        ReadOnlyFolderHiddenDesktopIni,
        SystemFolderHiddenDesktopIni
    ];
}
