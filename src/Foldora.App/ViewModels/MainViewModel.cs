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
    private readonly AsyncRelayCommand saveCommand;
    private readonly AsyncRelayCommand dryRunCommand;
    private readonly AsyncRelayCommand registerExplorerCommand;
    private readonly AsyncRelayCommand unregisterExplorerCommand;
    private readonly AsyncRelayCommand resetMenuCommand;
    private readonly RelayCommand reloadCommand;
    private readonly RelayCommand addEntryCommand;
    private string title = "Создать папку";
    private string statusMessage = "Загрузка настроек...";
    private bool explorerIntegrationEnabled;
    private bool hasUnsavedChanges;
    private bool isResetConfirmed;

    public MainViewModel(
        FolderMenuDraftEditor draftEditor,
        IIconFilePicker iconFilePicker,
        IIconPreviewService iconPreviewService,
        ExplorerIntegrationController explorerIntegrationController)
    {
        this.draftEditor = draftEditor ?? throw new ArgumentNullException(nameof(draftEditor));
        this.iconFilePicker = iconFilePicker ?? throw new ArgumentNullException(nameof(iconFilePicker));
        this.iconPreviewService = iconPreviewService ?? throw new ArgumentNullException(nameof(iconPreviewService));
        this.explorerIntegrationController = explorerIntegrationController ?? throw new ArgumentNullException(nameof(explorerIntegrationController));

        saveCommand = new AsyncRelayCommand(SaveAsync, () => HasUnsavedChanges);
        dryRunCommand = new AsyncRelayCommand(DryRunExplorerIntegrationAsync);
        registerExplorerCommand = new AsyncRelayCommand(RegisterExplorerIntegrationAsync);
        unregisterExplorerCommand = new AsyncRelayCommand(UnregisterExplorerIntegrationAsync);
        resetMenuCommand = new AsyncRelayCommand(ResetMenuAsync, () => IsResetConfirmed);
        reloadCommand = new RelayCommand(ReloadDraft, () => HasUnsavedChanges);
        addEntryCommand = new RelayCommand(AddEntry);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<FolderMenuEntryViewModel> Entries { get; } = [];

    public ObservableCollection<string> Errors { get; } = [];

    public ObservableCollection<string> OperationDetails { get; } = [];

    public AsyncRelayCommand SaveCommand => saveCommand;

    public AsyncRelayCommand DryRunCommand => dryRunCommand;

    public AsyncRelayCommand RegisterExplorerCommand => registerExplorerCommand;

    public AsyncRelayCommand UnregisterExplorerCommand => unregisterExplorerCommand;

    public AsyncRelayCommand ResetMenuCommand => resetMenuCommand;

    public RelayCommand ReloadCommand => reloadCommand;

    public RelayCommand AddEntryCommand => addEntryCommand;

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
        }
    }

    public string ExplorerIntegrationStatusText => ExplorerIntegrationEnabled ? "Включена" : "Отключена";

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
            resetMenuCommand.RaiseCanExecuteChanged();
        }
    }

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
            saveCommand.RaiseCanExecuteChanged();
            reloadCommand.RaiseCanExecuteChanged();
        }
    }

    public bool HasErrors => Errors.Count > 0;

    public static MainViewModel CreateDefault()
    {
        var paths = FoldoraDataPaths.CreateDefault();
        var storage = new FoldoraSettingsStorage(paths);
        var draftEditor = new FolderMenuDraftEditor(storage, paths);
        var registrationService = new ExplorerMenuRegistrationService(
            storage,
            new ExplorerMenuRegistryPlanBuilder(),
            new ExplorerMenuRegistryWriter(new WindowsRegistryAccess()));
        var integrationController = new ExplorerIntegrationController(
            draftEditor,
            registrationService,
            new ExplorerCliPathResolver());

        return new MainViewModel(
            draftEditor,
            new WindowsIconFilePicker(),
            new WpfIconPreviewService(),
            integrationController);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await draftEditor.LoadAsync(cancellationToken);
        LoadDraftIntoViewModels();
        Errors.Clear();
        OperationDetails.Clear();
        OnPropertyChanged(nameof(HasErrors));
        StatusMessage = Entries.Count == 0
            ? "Пункты меню не настроены. Пустое меню является нормальным состоянием."
            : "Настройки загружены.";
        ExplorerIntegrationEnabled = draftEditor.ExplorerIntegrationEnabled;
        RefreshDirtyState();
    }

    private async Task SaveAsync()
    {
        Errors.Clear();
        OperationDetails.Clear();
        var result = await draftEditor.SaveAsync();
        if (!result.Saved)
        {
            foreach (var issue in result.Issues.Where(issue => issue.Severity == FolderMenuValidationSeverity.Error))
            {
                Errors.Add(FormatIssue(issue));
            }

            OnPropertyChanged(nameof(HasErrors));
            StatusMessage = "Настройки не сохранены. Исправьте ошибки.";
            RefreshDirtyState();
            return;
        }

        LoadDraftIntoViewModels();
        OnPropertyChanged(nameof(HasErrors));
        StatusMessage = draftEditor.ExplorerIntegrationEnabled
            ? "Настройки сохранены. Меню Проводника не обновлялось в этом шаге."
            : "Настройки сохранены.";
        ExplorerIntegrationEnabled = draftEditor.ExplorerIntegrationEnabled;
        RefreshDirtyState();
    }

    private void ReloadDraft()
    {
        draftEditor.Reload();
        LoadDraftIntoViewModels();
        Errors.Clear();
        OperationDetails.Clear();
        OnPropertyChanged(nameof(HasErrors));
        StatusMessage = "Несохранённые изменения отменены.";
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

        OnPropertyChanged(nameof(HasErrors));
        ExplorerIntegrationEnabled = result.ExplorerIntegrationEnabled;
        StatusMessage = result.Message;
        RefreshDirtyState();
    }

    private void AddEntry()
    {
        var entry = draftEditor.AddEntry();
        Entries.Add(CreateEntryViewModel(entry));
        Errors.Clear();
        OperationDetails.Clear();
        OnPropertyChanged(nameof(HasErrors));
        StatusMessage = "Добавлен draft-пункт. Выберите .ico перед сохранением.";
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

            OnPropertyChanged(nameof(HasErrors));
            StatusMessage = "Иконка не выбрана. Исправьте ошибку файла.";
            return;
        }

        entry.RefreshIconState();
        OperationDetails.Clear();
        OnPropertyChanged(nameof(HasErrors));
        StatusMessage = "Иконка выбрана и будет импортирована при сохранении.";
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
        OnPropertyChanged(nameof(HasErrors));
        StatusMessage = "Пункт удалён из draft. Файлы и настройки изменятся только после сохранения.";
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
    }

    private FolderMenuEntryViewModel CreateEntryViewModel(FolderMenuDraftEntry entry)
    {
        return new FolderMenuEntryViewModel(entry, iconPreviewService, RefreshDirtyState, ChooseIconAsync, RemoveEntry);
    }

    private void RefreshDirtyState()
    {
        HasUnsavedChanges = draftEditor.HasUnsavedChanges;
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
}
