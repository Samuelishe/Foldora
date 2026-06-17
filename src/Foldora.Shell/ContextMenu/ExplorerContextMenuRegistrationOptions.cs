namespace Foldora.Shell.ContextMenu;

/// <summary>
/// Параметры будущей регистрации legacy context menu в HKCU.
/// </summary>
public sealed record ExplorerContextMenuRegistrationOptions(
    string CliExecutablePath,
    string MenuTitle = "Foldora");
