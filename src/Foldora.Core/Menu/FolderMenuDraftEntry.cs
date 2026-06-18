using Foldora.Core.Validation;

namespace Foldora.Core.Menu;

/// <summary>
/// Редактируемая draft-копия пользовательского пункта меню.
/// </summary>
public sealed class FolderMenuDraftEntry
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string DefaultFolderName { get; set; } = FolderNameValidator.DefaultFolderName;

    public string IconPath { get; set; } = string.Empty;

    public string? PendingIconSourcePath { get; set; }

    public string? PreviewPath { get; init; }

    public int SortOrder { get; init; }

    public bool IsEnabled { get; set; } = true;

    public static FolderMenuDraftEntry FromEntry(FolderMenuEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return new FolderMenuDraftEntry
        {
            Id = entry.Id,
            DisplayName = entry.DisplayName,
            DefaultFolderName = entry.DefaultFolderName,
            IconPath = entry.IconPath,
            PreviewPath = entry.PreviewPath,
            SortOrder = entry.SortOrder,
            IsEnabled = entry.IsEnabled
        };
    }

    public FolderMenuEntry ToEntry()
    {
        return ToEntry(IconPath);
    }

    public FolderMenuEntry ToEntry(string iconPath)
    {
        return new FolderMenuEntry
        {
            Id = Id,
            DisplayName = DisplayName,
            DefaultFolderName = DefaultFolderName,
            IconPath = iconPath,
            PreviewPath = PreviewPath,
            SortOrder = SortOrder,
            IsEnabled = IsEnabled
        };
    }
}
