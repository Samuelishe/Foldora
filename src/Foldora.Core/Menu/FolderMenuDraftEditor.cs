using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Core.Validation;

namespace Foldora.Core.Menu;

/// <summary>
/// Staged-save редактор пользовательского меню без WPF- и registry-зависимостей.
/// </summary>
public sealed class FolderMenuDraftEditor
{
    private readonly FoldoraSettingsStorage settingsStorage;
    private readonly FoldoraDataPaths paths;
    private readonly FolderMenuSettingsValidator settingsValidator;
    private readonly IconFileValidator iconFileValidator;
    private readonly IconImportService iconImportService;
    private FoldoraSettings savedSettings = new();

    public FolderMenuDraftEditor(
        FoldoraSettingsStorage settingsStorage,
        FoldoraDataPaths paths,
        FolderMenuSettingsValidator? settingsValidator = null,
        IconFileValidator? iconFileValidator = null,
        IconImportService? iconImportService = null)
    {
        this.settingsStorage = settingsStorage ?? throw new ArgumentNullException(nameof(settingsStorage));
        this.paths = paths ?? throw new ArgumentNullException(nameof(paths));
        this.settingsValidator = settingsValidator ?? new FolderMenuSettingsValidator();
        this.iconFileValidator = iconFileValidator ?? new IconFileValidator();
        this.iconImportService = iconImportService ?? new IconImportService(this.iconFileValidator);
    }

    public string Title { get; set; } = FolderMenuSettings.CreateDefault().Title;

    public List<FolderMenuDraftEntry> Entries { get; } = [];

    public bool HasUnsavedChanges => Entries.Any(entry => !string.IsNullOrWhiteSpace(entry.PendingIconSourcePath))
                                     || !SettingsEquals(BuildDraftSettings(), savedSettings);

    public bool ExplorerIntegrationEnabled => savedSettings.ExplorerIntegrationEnabled;

    public string Language => savedSettings.Language;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        savedSettings = await settingsStorage.LoadAsync(cancellationToken);
        ResetDraftFromSaved();
    }

    public void Reload()
    {
        ResetDraftFromSaved();
    }

    public FolderMenuDraftEntry AddEntry()
    {
        var entry = new FolderMenuDraftEntry
        {
            Id = CreateEntryId(),
            DisplayName = FolderMenuNameGenerator.GetNextName(Entries.Select(entry => entry.ToEntry())),
            DefaultFolderName = FolderNameValidator.DefaultFolderName,
            IsEnabled = true,
            SortOrder = Entries.Count == 0 ? 0 : Entries.Max(entry => entry.SortOrder) + 1
        };

        Entries.Add(entry);
        return entry;
    }

    public bool RemoveEntry(string entryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);

        var entry = Entries.FirstOrDefault(entry => string.Equals(entry.Id, entryId, StringComparison.OrdinalIgnoreCase));
        if (entry is null)
        {
            return false;
        }

        Entries.Remove(entry);
        return true;
    }

    public FolderMenuValidationResult SetPendingIconSource(string entryId, string sourceIconPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceIconPath);

        var entry = Entries.FirstOrDefault(entry => string.Equals(entry.Id, entryId, StringComparison.OrdinalIgnoreCase));
        if (entry is null)
        {
            return new FolderMenuValidationResult(
                [new FolderMenuValidationIssue(FolderMenuValidationSeverity.Error, "Menu entry was not found.", "entry_not_found", entryId)]);
        }

        var validation = iconFileValidator.Validate(sourceIconPath);
        if (!validation.IsValid)
        {
            return WithEntryId(validation, entryId);
        }

        entry.PendingIconSourcePath = sourceIconPath;
        return FolderMenuValidationResult.Success;
    }

    public FolderMenuValidationResult ValidateDraft()
    {
        return ValidateDraftForSave(BuildDraftSettingsForSave());
    }

    public async Task<FolderMenuDraftSaveResult> SaveAsync(CancellationToken cancellationToken = default)
    {
        var draftSettings = BuildDraftSettingsForSave();
        var validation = ValidateDraftForSave(draftSettings);
        if (!validation.IsValid)
        {
            return FolderMenuDraftSaveResult.Blocked(validation);
        }

        foreach (var entry in Entries.Where(entry => !string.IsNullOrWhiteSpace(entry.PendingIconSourcePath)))
        {
            var importedIconPath = await iconImportService.ImportForEntryAsync(
                entry.PendingIconSourcePath!,
                paths,
                entry.Id,
                cancellationToken);

            var savedEntry = draftSettings.CreateFolderMenu.Entries.First(savedEntry => savedEntry.Id == entry.Id);
            savedEntry.IconPath = importedIconPath;
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

    private FoldoraSettings BuildDraftSettingsForSave()
    {
        return savedSettings with { CreateFolderMenu = BuildDraftMenuSettings(GetEffectiveIconPathForSave) };
    }

    private FolderMenuSettings BuildDraftMenuSettings()
    {
        return BuildDraftMenuSettings(entry => entry.IconPath);
    }

    private FolderMenuSettings BuildDraftMenuSettings(Func<FolderMenuDraftEntry, string> iconPathSelector)
    {
        return new FolderMenuSettings
        {
            Title = Title,
            Entries = Entries.Select(entry => entry.ToEntry(iconPathSelector(entry))).ToList()
        };
    }

    private FolderMenuValidationResult ValidateDraftForSave(FoldoraSettings draftSettings)
    {
        var issues = new List<FolderMenuValidationIssue>();
        issues.AddRange(settingsValidator.Validate(draftSettings.CreateFolderMenu).Issues);

        foreach (var entry in Entries.Where(entry => entry.IsEnabled))
        {
            if (!string.IsNullOrWhiteSpace(entry.PendingIconSourcePath))
            {
                issues.AddRange(WithEntryId(iconFileValidator.Validate(entry.PendingIconSourcePath), entry.Id).Issues);
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.IconPath))
            {
                continue;
            }

            issues.AddRange(WithEntryId(iconFileValidator.Validate(entry.IconPath), entry.Id).Issues);
        }

        return issues.Count == 0 ? FolderMenuValidationResult.Success : new FolderMenuValidationResult(issues);
    }

    private string GetEffectiveIconPathForSave(FolderMenuDraftEntry entry)
    {
        return string.IsNullOrWhiteSpace(entry.PendingIconSourcePath)
            ? entry.IconPath
            : GetImportedIconPath(entry.Id);
    }

    private string GetImportedIconPath(string entryId)
    {
        return Path.Combine(paths.IconsDirectory, $"{entryId}.ico");
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
               && string.Equals(left.GroupName, right.GroupName, StringComparison.Ordinal)
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

    private static string CreateEntryId()
    {
        return $"entry-{Guid.NewGuid():N}";
    }

    private static FolderMenuValidationResult WithEntryId(FolderMenuValidationResult validation, string entryId)
    {
        if (validation.IsValid)
        {
            return validation;
        }

        return new FolderMenuValidationResult(validation.Issues.Select(issue => issue with { EntryId = entryId }));
    }
}
