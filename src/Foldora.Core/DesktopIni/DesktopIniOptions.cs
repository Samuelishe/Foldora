namespace Foldora.Core.DesktopIni;

/// <summary>
/// Параметры записи иконки в desktop.ini.
/// </summary>
public sealed record DesktopIniOptions(
    string FolderPath,
    string IconPath,
    DesktopIniAttributePolicy? AttributePolicy = null);
