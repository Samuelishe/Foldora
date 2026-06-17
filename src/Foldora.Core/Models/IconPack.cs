namespace Foldora.Core.Models;

/// <summary>
/// Описание набора пользовательских иконок папок.
/// </summary>
public sealed record IconPack(
    string Id,
    string DisplayName,
    string Version,
    string Author,
    string? Source,
    IReadOnlyList<FolderIconStyle> Styles);
