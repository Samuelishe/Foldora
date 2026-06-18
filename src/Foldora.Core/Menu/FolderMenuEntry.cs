using Foldora.Core.Validation;

namespace Foldora.Core.Menu;

/// <summary>
/// Пользовательский пункт будущего меню создания папки.
/// </summary>
public sealed class FolderMenuEntry
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string DefaultFolderName { get; set; } = FolderNameValidator.DefaultFolderName;

    public string GroupName { get; set; } = string.Empty;

    public string IconPath { get; set; } = string.Empty;

    public string? PreviewPath { get; set; }

    public int SortOrder { get; set; }

    public bool IsEnabled { get; set; } = true;
}
