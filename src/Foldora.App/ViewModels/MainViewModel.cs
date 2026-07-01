using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Core.Validation;
using Foldora.App.Services;
using Foldora.Shell.Registry;
using Foldora.Shell.RegistryPlan;

namespace Foldora.App.ViewModels;

/// <summary>
/// ViewModel главного окна WPF editor с staged editing и явными Explorer integration commands.
/// </summary>
public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly FolderMenuDraftEditor draftEditor;
    private readonly IIconFilePicker iconFilePicker;
    private readonly IIconPreviewService iconPreviewService;
    private readonly ExplorerIntegrationController explorerIntegrationController;
    private readonly ISettingsDialogService settingsDialogService;
    private readonly ILocalizationService localizationService;
    private readonly AsyncRelayCommand saveCommand;
    private readonly AsyncRelayCommand openSettingsCommand;
    private readonly AsyncRelayCommand dryRunCommand;
    private readonly AsyncRelayCommand registerExplorerCommand;
    private readonly AsyncRelayCommand unregisterExplorerCommand;
    private readonly AsyncRelayCommand resetMenuCommand;
    private readonly RelayCommand reloadCommand;
    private readonly RelayCommand addEntryCommand;
    private readonly RelayCommand addGroupCommand;
    private string title = string.Empty;
    private string statusMessage = string.Empty;
    private bool explorerIntegrationEnabled;
    private bool hasUnsavedChanges;
    private bool isResetConfirmed;
    private bool showTechnicalDetails;

    public MainViewModel(
        FolderMenuDraftEditor draftEditor,
        IIconFilePicker iconFilePicker,
        IIconPreviewService iconPreviewService,
        ExplorerIntegrationController explorerIntegrationController,
        ISettingsDialogService? settingsDialogService = null,
        ILocalizationService? localizationService = null)
    {
        this.draftEditor = draftEditor ?? throw new ArgumentNullException(nameof(draftEditor));
        this.iconFilePicker = iconFilePicker ?? throw new ArgumentNullException(nameof(iconFilePicker));
        this.iconPreviewService = iconPreviewService ?? throw new ArgumentNullException(nameof(iconPreviewService));
        this.explorerIntegrationController = explorerIntegrationController ?? throw new ArgumentNullException(nameof(explorerIntegrationController));
        this.settingsDialogService = settingsDialogService ?? new NoopSettingsDialogService();
        this.localizationService = localizationService ?? new InMemoryLocalizationService();
        title = L.CreateFolderMenuTitle;
        statusMessage = L.LoadingSettings;

        saveCommand = new AsyncRelayCommand(SaveDraftAsync, () => HasUnsavedChanges);
        openSettingsCommand = new AsyncRelayCommand(OpenSettingsAsync);
        dryRunCommand = new AsyncRelayCommand(DryRunExplorerIntegrationAsync);
        registerExplorerCommand = new AsyncRelayCommand(RegisterExplorerIntegrationAsync);
        unregisterExplorerCommand = new AsyncRelayCommand(UnregisterExplorerIntegrationAsync);
        resetMenuCommand = new AsyncRelayCommand(ResetMenuAsync, () => IsResetConfirmed);
        reloadCommand = new RelayCommand(ReloadDraft, () => HasUnsavedChanges);
        addEntryCommand = new RelayCommand(AddEntry);
        addGroupCommand = new RelayCommand(AddGroup);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<FolderMenuEntryViewModel> Entries { get; } = [];

    public ObservableCollection<FolderMenuEntryGroupViewModel> EntryGroups { get; } = [];

    public ObservableCollection<string> Errors { get; } = [];

    public ObservableCollection<string> OperationDetails { get; } = [];

    public ObservableCollection<string> TechnicalDetails => OperationDetails;

    public LocalizationResources L => localizationService.Resources;

    public AsyncRelayCommand SaveCommand => saveCommand;

    public AsyncRelayCommand OpenSettingsCommand => openSettingsCommand;

    public AsyncRelayCommand DryRunCommand => dryRunCommand;

    public AsyncRelayCommand RegisterExplorerCommand => registerExplorerCommand;

    public AsyncRelayCommand UnregisterExplorerCommand => unregisterExplorerCommand;

    public AsyncRelayCommand ResetMenuCommand => resetMenuCommand;

    public RelayCommand ReloadCommand => reloadCommand;

    public RelayCommand AddEntryCommand => addEntryCommand;

    public RelayCommand AddGroupCommand => addGroupCommand;

    public string Title
    {
        get => title;
        set
        {
            if (title == value)
            {
                return;
            }

            title = value;
            draftEditor.Title = value;
            OnPropertyChanged();
            RefreshDirtyState();
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set
        {
            if (statusMessage == value)
            {
                return;
            }

            statusMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasStatusMessage));
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public string ExplorerIntegrationStatusText => ExplorerIntegrationEnabled ? L.ExplorerEnabled : L.ExplorerDisabled;

    public string ExplorerIntegrationStatusLabel => string.Format(L.StatusLabelFormat, ExplorerIntegrationStatusText);

    public string UnsavedChangesText => string.Format(L.UnsavedChangesFormat, HasUnsavedChanges);

    public bool ExplorerIntegrationEnabled
    {
        get => explorerIntegrationEnabled;
        private set
        {
            if (explorerIntegrationEnabled == value)
            {
                return;
            }

            explorerIntegrationEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExplorerIntegrationStatusText));
            OnPropertyChanged(nameof(ExplorerIntegrationStatusLabel));
        }
    }

    public bool IsResetConfirmed
    {
        get => isResetConfirmed;
        set
        {
            if (isResetConfirmed == value)
            {
                return;
            }

            isResetConfirmed = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanResetMenu));
            resetMenuCommand.RaiseCanExecuteChanged();
        }
    }

    public bool CanResetMenu => IsResetConfirmed;

    public bool HasUnsavedChanges
    {
        get => hasUnsavedChanges;
        private set
        {
            if (hasUnsavedChanges == value)
            {
                return;
            }

            hasUnsavedChanges = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UnsavedChangesText));
            saveCommand.RaiseCanExecuteChanged();
            reloadCommand.RaiseCanExecuteChanged();
        }
    }

    public bool HasEntries => Entries.Count > 0;

    public bool HasEntryGroups => EntryGroups.Count > 0;

    public bool HasErrors => Errors.Count > 0;

    public bool HasValidationErrors => HasErrors;

    public bool HasTechnicalDetails => OperationDetails.Count > 0;

    public bool ShowTechnicalDetails
    {
        get => showTechnicalDetails;
        set
        {
            if (showTechnicalDetails == value)
            {
                return;
            }

            showTechnicalDetails = value;
            OnPropertyChanged();
        }
    }

    public static MainViewModel CreateDefault()
    {
        var paths = FoldoraDataPaths.CreateDefault();
        var storage = new FoldoraSettingsStorage(paths);
        var draftEditor = new FolderMenuDraftEditor(storage, paths);
        var localizationService = new InMemoryLocalizationService();
        var registrationService = new ExplorerMenuRegistrationService(
            storage,
            new ExplorerMenuRegistryPlanBuilder(),
            new ExplorerMenuRegistryWriter(new WindowsRegistryAccess()));
        var integrationController = new ExplorerIntegrationController(
            draftEditor,
            registrationService,
            new ExplorerCommandHostPathResolver(),
            localizationService);

        return new MainViewModel(
            draftEditor,
            new WindowsIconFilePicker(localizationService),
            new WpfIconPreviewService(),
            integrationController,
            new WindowSettingsDialogService(storage, localizationService),
            localizationService);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await draftEditor.LoadAsync(cancellationToken);
        localizationService.SetLanguage(draftEditor.Language);
        OnPropertyChanged(nameof(L));
        RefreshLocalizedEntryState();
        OnPropertyChanged(nameof(ExplorerIntegrationStatusText));
        OnPropertyChanged(nameof(ExplorerIntegrationStatusLabel));
        OnPropertyChanged(nameof(UnsavedChangesText));
        LoadDraftIntoViewModels();
        Errors.Clear();
        OperationDetails.Clear();
        NotifyErrorAndDetailsStateChanged();
        StatusMessage = Entries.Count == 0
            ? L.EmptyMenuStatus
            : L.SettingsLoaded;
        ExplorerIntegrationEnabled = draftEditor.ExplorerIntegrationEnabled;
        RefreshDirtyState();
    }

    public async Task SaveDraftAsync()
    {
        Errors.Clear();
        OperationDetails.Clear();
        ClearInlineEntryErrors();
        var shouldRebuildExplorerMenu = draftEditor.ExplorerIntegrationEnabled;
        var result = await draftEditor.SaveAsync();
        if (!result.Saved)
        {
            foreach (var issue in result.Issues.Where(issue => issue.Severity == FolderMenuValidationSeverity.Error))
            {
                Errors.Add(FormatIssue(issue));
                AddInlineEntryError(issue);
            }

            NotifyErrorAndDetailsStateChanged();
            StatusMessage = L.SettingsNotSavedFixErrors;
            RefreshDirtyState();
            return;
        }

        LoadDraftIntoViewModels();
        NotifyErrorAndDetailsStateChanged();
        ExplorerIntegrationEnabled = draftEditor.ExplorerIntegrationEnabled;

        if (shouldRebuildExplorerMenu)
        {
            var rebuildResult = await explorerIntegrationController.RebuildAfterSaveAsync();
            ApplyIntegrationResult(rebuildResult, reloadDraft: rebuildResult.Success);
            return;
        }

        StatusMessage = L.SettingsSaved;
        RefreshDirtyState();
    }

    private void ReloadDraft()
    {
        draftEditor.Reload();
        LoadDraftIntoViewModels();
        Errors.Clear();
        OperationDetails.Clear();
        NotifyErrorAndDetailsStateChanged();
        StatusMessage = L.UnsavedChangesDiscarded;
        ExplorerIntegrationEnabled = draftEditor.ExplorerIntegrationEnabled;
        RefreshDirtyState();
    }

    private async Task DryRunExplorerIntegrationAsync()
    {
        var result = await explorerIntegrationController.DryRunAsync();
        ApplyIntegrationResult(result, reloadDraft: false);
    }

    private async Task RegisterExplorerIntegrationAsync()
    {
        var result = await explorerIntegrationController.RegisterAsync();
        ApplyIntegrationResult(result, reloadDraft: result.Success);
    }

    private async Task UnregisterExplorerIntegrationAsync()
    {
        var hadUnsavedChanges = HasUnsavedChanges;
        var result = await explorerIntegrationController.UnregisterAsync();
        ApplyIntegrationResult(result, reloadDraft: result.Success && !hadUnsavedChanges);
    }

    private async Task ResetMenuAsync()
    {
        var result = await explorerIntegrationController.ResetMenuAsync();
        IsResetConfirmed = false;
        ApplyIntegrationResult(result, reloadDraft: result.Success);
    }

    public async Task OpenSettingsAsync()
    {
        var result = await settingsDialogService.ShowSettingsAsync();
        if (!result.Changed)
        {
            return;
        }

        localizationService.SetLanguage(result.Language);
        OnPropertyChanged(nameof(L));
        RefreshLocalizedEntryState();
        RebuildEntryGroups();
        StatusMessage = L.LanguageSavedRestartNotice;
        OnPropertyChanged(nameof(ExplorerIntegrationStatusText));
        OnPropertyChanged(nameof(ExplorerIntegrationStatusLabel));
        OnPropertyChanged(nameof(UnsavedChangesText));
    }

    private void ApplyIntegrationResult(ExplorerIntegrationOperationResult result, bool reloadDraft)
    {
        Errors.Clear();
        OperationDetails.Clear();

        if (reloadDraft)
        {
            LoadDraftIntoViewModels();
        }

        foreach (var detail in result.Details)
        {
            OperationDetails.Add(detail);
        }

        if (!result.Success)
        {
            Errors.Add(result.Message);
        }

        NotifyErrorAndDetailsStateChanged();
        ExplorerIntegrationEnabled = result.ExplorerIntegrationEnabled;
        StatusMessage = result.Message;
        RefreshDirtyState();
    }

    private void AddEntry()
    {
        AddEntryToGroup(string.Empty);
    }

    private void AddGroup()
    {
        AddEntryToGroup(GetNextGroupName());
    }

    private void AddEntryToGroup(string groupName)
    {
        var entry = draftEditor.AddEntry(GetNextEntryDisplayName(), L.DefaultFolderName);
        entry.GroupName = groupName;
        var entryViewModel = CreateEntryViewModel(entry);
        Entries.Add(entryViewModel);
        Errors.Clear();
        OperationDetails.Clear();
        RebuildEntryGroups();
        RequestEntryEdit(entryViewModel);
        NotifyEntryStateChanged();
        NotifyErrorAndDetailsStateChanged();
        StatusMessage = L.DraftEntryAddedChooseIcon;
        RefreshDirtyState();
    }

    private async Task ChooseIconAsync(FolderMenuEntryViewModel entry)
    {
        var result = iconFilePicker.PickIcon();
        if (!result.Selected)
        {
            return;
        }

        Errors.Clear();
        var validation = draftEditor.SetPendingIconSource(entry.Id, result.FilePath!);
        if (!validation.IsValid)
        {
            OperationDetails.Clear();
            foreach (var issue in validation.Issues.Where(issue => issue.Severity == FolderMenuValidationSeverity.Error))
            {
                Errors.Add(FormatIssue(issue));
            }

            NotifyErrorAndDetailsStateChanged();
            StatusMessage = L.IconNotSelectedFixFile;
            return;
        }

        entry.RefreshIconState();
        OperationDetails.Clear();
        NotifyErrorAndDetailsStateChanged();
        StatusMessage = L.IconSelectedImportedOnSave;
        RefreshDirtyState();
        await Task.CompletedTask;
    }

    private void RemoveEntry(FolderMenuEntryViewModel entry)
    {
        if (!draftEditor.RemoveEntry(entry.Id))
        {
            return;
        }

        Entries.Remove(entry);
        Errors.Clear();
        OperationDetails.Clear();
        RebuildEntryGroups();
        NotifyEntryStateChanged();
        NotifyErrorAndDetailsStateChanged();
        StatusMessage = L.EntryRemovedDraft;
        RefreshDirtyState();
    }

    private void LoadDraftIntoViewModels()
    {
        title = draftEditor.Title;
        OnPropertyChanged(nameof(Title));

        Entries.Clear();
        foreach (var entry in draftEditor.Entries)
        {
            Entries.Add(CreateEntryViewModel(entry));
        }

        RebuildEntryGroups();
        NotifyEntryStateChanged();
    }

    private FolderMenuEntryViewModel CreateEntryViewModel(FolderMenuDraftEntry entry)
    {
        return new FolderMenuEntryViewModel(
            entry,
            iconPreviewService,
            RefreshDirtyState,
            OnEntryGroupChanged,
            RequestEntryEdit,
            ChooseIconAsync,
            RemoveEntry,
            () => L);
    }

    private void RequestEntryEdit(FolderMenuEntryViewModel entry)
    {
        foreach (var existingEntry in Entries.Where(existingEntry => !ReferenceEquals(existingEntry, entry) && !existingEntry.HasInlineErrors))
        {
            existingEntry.CollapseEditing();
        }

        entry.BeginEditing();
    }

    private void OnEntryGroupChanged()
    {
        RebuildEntryGroups();
        RefreshDirtyState();
    }

    private void RebuildEntryGroups()
    {
        EntryGroups.Clear();

        var rootEntries = Entries
            .Where(entry => string.IsNullOrWhiteSpace(entry.GroupName))
            .ToList();
        if (rootEntries.Count > 0)
        {
            EntryGroups.Add(new FolderMenuEntryGroupViewModel(
                string.Empty,
                L.RootGroupTitle,
                rootEntries,
                string.Format(L.EntryCountFormat, rootEntries.Count),
                L.AddEntry,
                L.DeleteGroup,
                L.ConfirmDeleteGroup,
                L.Cancel,
                isRootSection: true,
                AddEntryToGroup,
                DeleteGroup,
                RenameGroup));
        }

        foreach (var group in Entries
                     .Where(entry => !string.IsNullOrWhiteSpace(entry.GroupName))
                     .GroupBy(entry => entry.GroupName.Trim(), StringComparer.Ordinal)
                     .OrderBy(group => Entries.IndexOf(group.First()))
                     .ThenBy(group => group.Key, StringComparer.Ordinal))
        {
            EntryGroups.Add(new FolderMenuEntryGroupViewModel(
                group.Key,
                group.Key,
                group,
                string.Format(L.EntryCountFormat, group.Count()),
                L.AddEntryToThisGroup,
                L.DeleteGroup,
                L.ConfirmDeleteGroup,
                L.Cancel,
                isRootSection: false,
                AddEntryToGroup,
                DeleteGroup,
                RenameGroup));
        }

        OnPropertyChanged(nameof(EntryGroups));
        OnPropertyChanged(nameof(HasEntryGroups));
    }

    private string GetNextGroupName()
    {
        var existingNames = Entries
            .Select(entry => entry.GroupName.Trim())
            .Where(groupName => !string.IsNullOrWhiteSpace(groupName))
            .ToHashSet(StringComparer.Ordinal);

        for (var index = 1; index < 1000; index++)
        {
            var candidate = $"{L.NewGroupNameBase} {index}";
            if (!existingNames.Contains(candidate))
            {
                return candidate;
            }
        }

        return $"{L.NewGroupNameBase} {Guid.NewGuid():N}";
    }

    private string GetNextEntryDisplayName()
    {
        var existingNames = Entries
            .Select(entry => entry.DisplayName)
            .Where(displayName => !string.IsNullOrWhiteSpace(displayName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var index = 1; index < 1000; index++)
        {
            var candidate = $"{L.DefaultEntryDisplayNamePrefix} {index}";
            if (!existingNames.Contains(candidate))
            {
                return candidate;
            }
        }

        return $"{L.DefaultEntryDisplayNamePrefix} {Guid.NewGuid():N}";
    }

    private void RefreshLocalizedEntryState()
    {
        foreach (var entry in Entries)
        {
            entry.RefreshLocalizedState();
        }
    }

    private void DeleteGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return;
        }

        var entriesToRemove = Entries
            .Where(entry => string.Equals(entry.GroupName.Trim(), groupName.Trim(), StringComparison.Ordinal))
            .ToList();

        foreach (var entry in entriesToRemove)
        {
            draftEditor.RemoveEntry(entry.Id);
            Entries.Remove(entry);
        }

        Errors.Clear();
        OperationDetails.Clear();
        RebuildEntryGroups();
        NotifyEntryStateChanged();
        NotifyErrorAndDetailsStateChanged();
        StatusMessage = L.GroupRemovedDraft;
        RefreshDirtyState();
    }

    private void RenameGroup(string oldGroupName, string newGroupName)
    {
        if (string.IsNullOrWhiteSpace(oldGroupName) || string.Equals(oldGroupName, newGroupName, StringComparison.Ordinal))
        {
            return;
        }

        foreach (var entry in Entries.Where(entry => string.Equals(entry.GroupName.Trim(), oldGroupName.Trim(), StringComparison.Ordinal)))
        {
            entry.GroupName = newGroupName;
        }

        RebuildEntryGroups();
        RefreshDirtyState();
    }

    private void RefreshDirtyState()
    {
        HasUnsavedChanges = draftEditor.HasUnsavedChanges;
    }

    private void ClearInlineEntryErrors()
    {
        foreach (var entry in Entries)
        {
            entry.ClearInlineErrors();
        }
    }

    private void AddInlineEntryError(FolderMenuValidationIssue issue)
    {
        if (string.IsNullOrWhiteSpace(issue.EntryId))
        {
            return;
        }

        var entry = Entries.FirstOrDefault(entry => string.Equals(entry.Id, issue.EntryId, StringComparison.Ordinal));
        entry?.AddInlineError(issue.Message);
    }

    private void NotifyEntryStateChanged()
    {
        OnPropertyChanged(nameof(HasEntries));
        OnPropertyChanged(nameof(HasEntryGroups));
    }

    private void NotifyErrorAndDetailsStateChanged()
    {
        OnPropertyChanged(nameof(HasErrors));
        OnPropertyChanged(nameof(HasValidationErrors));
        OnPropertyChanged(nameof(HasTechnicalDetails));
    }

    private static string FormatIssue(FolderMenuValidationIssue issue)
    {
        return string.IsNullOrWhiteSpace(issue.EntryId)
            ? issue.Message
            : $"{issue.EntryId}: {issue.Message}";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed class NoopSettingsDialogService : ISettingsDialogService
    {
        public Task<SettingsDialogResult> ShowSettingsAsync()
        {
            return Task.FromResult(new SettingsDialogResult(false, FoldoraLanguage.Russian));
        }
    }
}
