using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Core.Validation;
using Foldora.App.Services;

namespace Foldora.App.ViewModels;

/// <summary>
/// ViewModel главного окна phase 1: staged editing существующих menu entries.
/// </summary>
public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly FolderMenuDraftEditor draftEditor;
    private readonly IIconFilePicker iconFilePicker;
    private readonly IIconPreviewService iconPreviewService;
    private readonly AsyncRelayCommand saveCommand;
    private readonly RelayCommand reloadCommand;
    private readonly RelayCommand addEntryCommand;
    private string title = "Создать папку";
    private string statusMessage = "Загрузка настроек...";
    private bool hasUnsavedChanges;

    public MainViewModel(
        FolderMenuDraftEditor draftEditor,
        IIconFilePicker iconFilePicker,
        IIconPreviewService iconPreviewService)
    {
        this.draftEditor = draftEditor ?? throw new ArgumentNullException(nameof(draftEditor));
        this.iconFilePicker = iconFilePicker ?? throw new ArgumentNullException(nameof(iconFilePicker));
        this.iconPreviewService = iconPreviewService ?? throw new ArgumentNullException(nameof(iconPreviewService));

        saveCommand = new AsyncRelayCommand(SaveAsync, () => HasUnsavedChanges);
        reloadCommand = new RelayCommand(ReloadDraft, () => HasUnsavedChanges);
        addEntryCommand = new RelayCommand(AddEntry);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<FolderMenuEntryViewModel> Entries { get; } = [];

    public ObservableCollection<string> Errors { get; } = [];

    public AsyncRelayCommand SaveCommand => saveCommand;

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
        return new MainViewModel(
            new FolderMenuDraftEditor(storage, paths),
            new WindowsIconFilePicker(),
            new WpfIconPreviewService());
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await draftEditor.LoadAsync(cancellationToken);
        LoadDraftIntoViewModels();
        Errors.Clear();
        OnPropertyChanged(nameof(HasErrors));
        StatusMessage = Entries.Count == 0
            ? "Пункты меню не настроены. Пустое меню является нормальным состоянием."
            : "Настройки загружены.";
        RefreshDirtyState();
    }

    private async Task SaveAsync()
    {
        Errors.Clear();
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
        RefreshDirtyState();
    }

    private void ReloadDraft()
    {
        draftEditor.Reload();
        LoadDraftIntoViewModels();
        Errors.Clear();
        OnPropertyChanged(nameof(HasErrors));
        StatusMessage = "Несохранённые изменения отменены.";
        RefreshDirtyState();
    }

    private void AddEntry()
    {
        var entry = draftEditor.AddEntry();
        Entries.Add(CreateEntryViewModel(entry));
        Errors.Clear();
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
            foreach (var issue in validation.Issues.Where(issue => issue.Severity == FolderMenuValidationSeverity.Error))
            {
                Errors.Add(FormatIssue(issue));
            }

            OnPropertyChanged(nameof(HasErrors));
            StatusMessage = "Иконка не выбрана. Исправьте ошибку файла.";
            return;
        }

        entry.RefreshIconState();
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
