using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Foldora.Core.Menu;
using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Core.Validation;

namespace Foldora.App.ViewModels;

/// <summary>
/// ViewModel главного окна phase 1: staged editing существующих menu entries.
/// </summary>
public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly FolderMenuDraftEditor draftEditor;
    private readonly AsyncRelayCommand saveCommand;
    private readonly RelayCommand reloadCommand;
    private string title = "Создать папку";
    private string statusMessage = "Загрузка настроек...";
    private bool hasUnsavedChanges;

    public MainViewModel(FolderMenuDraftEditor draftEditor)
    {
        this.draftEditor = draftEditor ?? throw new ArgumentNullException(nameof(draftEditor));

        saveCommand = new AsyncRelayCommand(SaveAsync, () => HasUnsavedChanges);
        reloadCommand = new RelayCommand(ReloadDraft, () => HasUnsavedChanges);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<FolderMenuEntryViewModel> Entries { get; } = [];

    public ObservableCollection<string> Errors { get; } = [];

    public AsyncRelayCommand SaveCommand => saveCommand;

    public RelayCommand ReloadCommand => reloadCommand;

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
        return new MainViewModel(new FolderMenuDraftEditor(storage));
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

    private void LoadDraftIntoViewModels()
    {
        title = draftEditor.Title;
        OnPropertyChanged(nameof(Title));

        Entries.Clear();
        foreach (var entry in draftEditor.Entries)
        {
            Entries.Add(new FolderMenuEntryViewModel(entry, RefreshDirtyState));
        }
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
