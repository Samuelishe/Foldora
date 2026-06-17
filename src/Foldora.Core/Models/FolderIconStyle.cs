namespace Foldora.Core.Models;

/// <summary>
/// Стиль иконки папки внутри набора.
/// </summary>
public sealed record FolderIconStyle(
    string Id,
    string DisplayName,
    string IconPath,
    string? Category,
    string? Description,
    int SortOrder);
