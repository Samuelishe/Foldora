using Foldora.Core.Settings;
using Foldora.Core.Validation;

namespace Foldora.Core.Menu;

/// <summary>
/// Staged-save редактор пользовательского меню без WPF- и registry-зависимостей.
/// </summary>
public sealed class FolderMenuDraftEditor
{
    private readonly FoldoraSettingsStorage settingsStorage;
    private readonly FolderMenuSettingsValidator settingsValidator;
    private FoldoraSettings savedSettings = new();

    public FolderMenuDraftEditor(
        FoldoraSettingsStorage settingsStorage,
        FolderMenuSettingsValidator? settingsValidator = null)
    {
        this.settingsStorage = settingsStorage ?? throw new ArgumentNullException(nameof(settingsStorage));
        this.settingsValidator = settingsValidator ?? new FolderMenuSettingsValidator();
    }

    public string Title { get; set; } = FolderMenuSettings.CreateDefault().Title;

    public List<FolderMenuDraftEntry> Entries { get; } = [];

    public bool HasUnsavedChanges => !SettingsEquals(BuildDraftSettings(), savedSettings);

    public bool ExplorerIntegrationEnabled => savedSettings.ExplorerIntegrationEnabled;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        savedSettings = await settingsStorage.LoadAsync(cancellationToken);
        ResetDraftFromSaved();
    }

    public void Reload()
    {
        ResetDraftFromSaved();
    }

    public FolderMenuValidationResult ValidateDraft()
    {
        return settingsValidator.Validate(BuildDraftMenuSettings());
    }

    public async Task<FolderMenuDraftSaveResult> SaveAsync(CancellationToken cancellationToken = default)
    {
        var draftSettings = BuildDraftSettings();
        var validation = settingsValidator.Validate(draftSettings.CreateFolderMenu);
        if (!validation.IsValid)
        {
            return FolderMenuDraftSaveResult.Blocked(validation);
        }

        await settingsStorage.SaveAsync(draftSettings, cancellationToken);
        savedSettings = draftSettings;
        ResetDraftFromSaved();

        return FolderMenuDraftSaveResult.Success;
    }

    private void ResetDraftFromSaved()
    {
        Title = savedSettings.CreateFolderMenu.Title;
        Entries.Clear();
        Entries.AddRange(savedSettings.CreateFolderMenu.Entries
            .OrderBy(entry => entry.SortOrder)
            .ThenBy(entry => entry.Id, StringComparer.OrdinalIgnoreCase)
            .Select(FolderMenuDraftEntry.FromEntry));
    }

    private FoldoraSettings BuildDraftSettings()
    {
        return savedSettings with { CreateFolderMenu = BuildDraftMenuSettings() };
    }

    private FolderMenuSettings BuildDraftMenuSettings()
    {
        return new FolderMenuSettings
        {
            Title = Title,
            Entries = Entries.Select(entry => entry.ToEntry()).ToList()
        };
    }

    private static bool SettingsEquals(FoldoraSettings left, FoldoraSettings right)
    {
        var leftMenu = left.CreateFolderMenu;
        var rightMenu = right.CreateFolderMenu;

        return left.ExplorerIntegrationEnabled == right.ExplorerIntegrationEnabled
               && string.Equals(leftMenu.Title, rightMenu.Title, StringComparison.Ordinal)
               && EntriesEqual(SortEntries(leftMenu.Entries), SortEntries(rightMenu.Entries));
    }

    private static bool EntriesEqual(IReadOnlyList<FolderMenuEntry> left, IReadOnlyList<FolderMenuEntry> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        for (var index = 0; index < left.Count; index++)
        {
            if (!EntryEquals(left[index], right[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool EntryEquals(FolderMenuEntry left, FolderMenuEntry right)
    {
        return string.Equals(left.Id, right.Id, StringComparison.Ordinal)
               && string.Equals(left.DisplayName, right.DisplayName, StringComparison.Ordinal)
               && string.Equals(left.DefaultFolderName, right.DefaultFolderName, StringComparison.Ordinal)
               && string.Equals(left.IconPath, right.IconPath, StringComparison.Ordinal)
               && string.Equals(left.PreviewPath, right.PreviewPath, StringComparison.Ordinal)
               && left.SortOrder == right.SortOrder
               && left.IsEnabled == right.IsEnabled;
    }

    private static FolderMenuEntry[] SortEntries(IEnumerable<FolderMenuEntry> entries)
    {
        return entries
            .OrderBy(entry => entry.SortOrder)
            .ThenBy(entry => entry.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
