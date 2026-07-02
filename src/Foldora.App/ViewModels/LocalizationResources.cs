using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Foldora.App.ViewModels;

/// <summary>
/// Набор локализованных строк для bindings в WPF.
/// </summary>
public sealed class LocalizationResources : INotifyPropertyChanged
{
    private IReadOnlyDictionary<string, string> values = new Dictionary<string, string>();

    public event PropertyChangedEventHandler? PropertyChanged;

    public string AppTitle => Get();
    public string PageTitle => Get();
    public string PageSubtitle => Get();
    public string MenuSettings => Get();
    public string MenuTitle => Get();
    public string MenuEntries => Get();
    public string AddEntry => Get();
    public string AddGroup => Get();
    public string RootGroupTitle => Get();
    public string AddEntryToGroupFormat => Get();
    public string AddEntryToThisGroup => Get();
    public string EntryCountFormat => Get();
    public string DeleteGroup => Get();
    public string DeleteGroupPrompt => Get();
    public string ConfirmDeleteGroup => Get();
    public string Cancel => Get();
    public string NewGroupNameBase => Get();
    public string EmptyTitle => Get();
    public string EmptyDescription => Get();
    public string EntryDisplayName => Get();
    public string EntryFolderName => Get();
    public string FolderNameSummaryLabel => Get();
    public string EntryGroupName => Get();
    public string EntryGroupHelp => Get();
    public string EntryEnabled => Get();
    public string EntryIcon => Get();
    public string ChooseIcon => Get();
    public string Delete => Get();
    public string Edit => Get();
    public string Done => Get();
    public string ExplorerIntegration => Get();
    public string StatusLabelFormat => Get();
    public string DryRun => Get();
    public string PreviewChanges => Get();
    public string PreviewChangesTooltip => Get();
    public string ExplorerMenuHelpTooltip => Get();
    public string RegisterExplorer => Get();
    public string UnregisterExplorer => Get();
    public string TechnicalDetails => Get();
    public string DangerZone => Get();
    public string ResetDescription => Get();
    public string ResetIconNote => Get();
    public string ResetConfirm => Get();
    public string ResetMenu => Get();
    public string UnsavedChangesFormat => Get();
    public string AllChangesSaved => Get();
    public string UnsavedChanges => Get();
    public string Reload => Get();
    public string Save => Get();
    public string Settings => Get();
    public string SettingsTabApplication => Get();
    public string SettingsTabExplorerMenu => Get();
    public string SettingsTabInstallation => Get();
    public string SettingsTabHelpAbout => Get();
    public string SettingsTabDangerZone => Get();
    public string ApplicationSection => Get();
    public string ExplorerMenuSection => Get();
    public string InstallationSection => Get();
    public string HelpAboutSection => Get();
    public string HelpAboutSummary => Get();
    public string OpenHelpAbout => Get();
    public string InstalledAppPath => Get();
    public string UserDataPath => Get();
    public string CurrentCommandHost => Get();
    public string InstallationPathsHelpTooltip => Get();
    public string OpenFolder => Get();
    public string OpenLocation => Get();
    public string CopyPath => Get();
    public string OpenFolderTooltip => Get();
    public string OpenLocationTooltip => Get();
    public string CopyPathTooltip => Get();
    public string PathCopiedFormat => Get();
    public string PathOpenFailedFormat => Get();
    public string PathCopyFailedFormat => Get();
    public string HelpTooltip => Get();
    public string MenuHostServiceNote => Get();
    public string ExplorerActionsApplyImmediately => Get();
    public string ManageInSettings => Get();
    public string SaveOrDiscardBeforeExplorer => Get();
    public string UninstallHint => Get();
    public string HelpWindowTitle => Get();
    public string HelpWhatFoldoraDoesTitle => Get();
    public string HelpWhatFoldoraDoesBody => Get();
    public string HelpHowToUseTitle => Get();
    public string HelpUseStepAddEntry => Get();
    public string HelpUseStepChooseIcon => Get();
    public string HelpUseStepSetNames => Get();
    public string HelpUseStepSave => Get();
    public string HelpUseStepEnableExplorer => Get();
    public string HelpUseStepUseExplorer => Get();
    public string HelpWhereMenuAppearsTitle => Get();
    public string HelpWhereMenuAppearsLegacy => Get();
    public string HelpWhereMenuAppearsHKCU => Get();
    public string HelpMenuHostTitle => Get();
    public string HelpMenuHostBody => Get();
    public string HelpDataInstallTitle => Get();
    public string HelpDataInstallBinaries => Get();
    public string HelpDataInstallUserData => Get();
    public string HelpDataInstallIcons => Get();
    public string HelpUninstallTitle => Get();
    public string HelpUninstallBody => Get();
    public string HelpUninstallUserDataWarning => Get();
    public string HelpLicenseTitle => Get();
    public string HelpLicenseBody => Get();
    public string HelpVersionFormat => Get();
    public string SettingsLanguageLabel => Get();
    public string SettingsLanguageRestartNote => Get();
    public string Minimize => Get();
    public string Maximize => Get();
    public string Close => Get();
    public string CreateFolderMenuTitle => Get();
    public string DefaultEntryDisplayNamePrefix => Get();
    public string DefaultFolderName => Get();
    public string UntitledEntry => Get();
    public string ExplorerEnabled => Get();
    public string ExplorerDisabled => Get();
    public string ExplorerMenuStatusOn => Get();
    public string ExplorerMenuStatusOff => Get();
    public string LoadingSettings => Get();
    public string EmptyMenuStatus => Get();
    public string SettingsLoaded => Get();
    public string SettingsNotSavedFixErrors => Get();
    public string SettingsSaved => Get();
    public string UnsavedChangesDiscarded => Get();
    public string LanguageSavedRestartNotice => Get();
    public string DraftEntryAddedChooseIcon => Get();
    public string IconNotSelectedFixFile => Get();
    public string IconSelectedImportedOnSave => Get();
    public string EntryRemovedDraft => Get();
    public string GroupRemovedDraft => Get();
    public string IconPreviewNotLoadedFormat => Get();
    public string IconWillBeUpdatedFormat => Get();
    public string IconNone => Get();
    public string IconMissing => Get();
    public string IconExistsFormat => Get();
    public string IconPreviewNotLoaded => Get();
    public string IconPickerTitle => Get();
    public string IconPickerFilterIconImages => Get();
    public string IconPickerFilterIco => Get();
    public string IconPickerFilterImages => Get();
    public string IconPickerFilterAllFiles => Get();
    public string IconImageConversionFailed => Get();
    public string IconDropTooltip => Get();
    public string IconDropMultipleFilesRejected => Get();
    public string IconDropUnsupportedFile => Get();
    public string IconDropDirectoryRejected => Get();
    public string IconDropCouldNotUseFile => Get();
    public string PreviewNoIcon => Get();
    public string PreviewIconMissing => Get();
    public string PreviewIconLoaded => Get();
    public string PreviewNotLoaded => Get();
    public string PlanChecked => Get();
    public string ExplorerMenuEnabled => Get();
    public string ExplorerNoEntriesNotCreated => Get();
    public string SettingsSavedExplorerUpdated => Get();
    public string SettingsSavedNoEntriesExplorerDisabled => Get();
    public string SettingsSavedExplorerNotUpdated => Get();
    public string ExplorerIntegrationErrorFormat => Get();
    public string ExplorerMenuDisabled => Get();
    public string MenuReset => Get();
    public string SaveUnsavedChangesFirst => Get();
    public string NoEnabledEntriesDetail => Get();

    public void Apply(IReadOnlyDictionary<string, string> newValues)
    {
        values = newValues;
        OnPropertyChanged(string.Empty);
    }

    public string this[string key] => values.TryGetValue(key, out var value) ? value : key;

    private string Get([CallerMemberName] string? key = null)
    {
        return key is not null && values.TryGetValue(key, out var value) ? value : key ?? string.Empty;
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
