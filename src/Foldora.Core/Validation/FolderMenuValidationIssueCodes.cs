namespace Foldora.Core.Validation;

/// <summary>
/// Стабильные invariant-коды проблем валидации пользовательского меню.
/// </summary>
public static class FolderMenuValidationIssueCodes
{
    public const string DisplayNameEmpty = "display_name_empty";
    public const string DisplayNameTooLong = "display_name_too_long";
    public const string DisplayNameControlChars = "display_name_control_chars";
    public const string FolderNameTooLong = "folder_name_too_long";
    public const string FolderNameControlChars = "folder_name_control_chars";
    public const string FolderNameInvalidChars = "folder_name_invalid_chars";
    public const string FolderNameReserved = "folder_name_reserved";
    public const string FolderNameTrailingDotOrSpace = "folder_name_trailing_dot_or_space";
    public const string GroupNameTooLong = "group_name_too_long";
    public const string GroupNameControlChars = "group_name_control_chars";
    public const string GroupNameNestedNotSupported = "group_name_nested_not_supported";
    public const string IconPathEmpty = "icon_path_empty";
    public const string IconMissing = "icon_missing";
    public const string IconExtension = "icon_extension";
    public const string IconEmpty = "icon_empty";
    public const string IconTooLarge = "icon_too_large";
    public const string IconNotReadable = "icon_not_readable";
    public const string IconReadFailed = "icon_read_failed";
    public const string IconHeaderTooSmall = "icon_header_too_small";
    public const string IconHeaderInvalid = "icon_header_invalid";
    public const string IconImageCountInvalid = "icon_image_count_invalid";
    public const string IconDirectoryOutOfBounds = "icon_directory_out_of_bounds";
    public const string IconDirectoryEntryIncomplete = "icon_directory_entry_incomplete";
    public const string IconImageEmpty = "icon_image_empty";
    public const string IconImageOffsetInvalid = "icon_image_offset_invalid";
    public const string IconImageDataOutOfBounds = "icon_image_data_out_of_bounds";
    public const string EntryIdEmpty = "entry_id_empty";
    public const string EntryNotFound = "entry_not_found";
    public const string EntryIconPathEmpty = "entry_icon_path_empty";
    public const string MenuTotalEntriesLimit = "menu_total_entries_limit";
    public const string MenuEnabledEntriesLimit = "menu_enabled_entries_limit";
    public const string MenuGroupLimit = "menu_group_limit";
    public const string MenuGroupChildrenLimit = "menu_group_children_limit";
}
