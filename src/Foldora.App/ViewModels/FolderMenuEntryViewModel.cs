using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Foldora.Core.Menu;

namespace Foldora.App.ViewModels;

/// <summary>
/// ViewModel draft-пункта меню для phase 1 WPF-редактора.
/// </summary>
public sealed class FolderMenuEntryViewModel : INotifyPropertyChanged
{
    private readonly FolderMenuDraftEntry draftEntry;
    private readonly Action changed;
    private readonly Func<FolderMenuEntryViewModel, Task> chooseIconAsync;
    private readonly Action<FolderMenuEntryViewModel> remove;

    public FolderMenuEntryViewModel(
        FolderMenuDraftEntry draftEntry,
        Action changed,
        Func<FolderMenuEntryViewModel, Task> chooseIconAsync,
        Action<FolderMenuEntryViewModel> remove)
    {
        this.draftEntry = draftEntry ?? throw new ArgumentNullException(nameof(draftEntry));
        this.changed = changed ?? throw new ArgumentNullException(nameof(changed));
        this.chooseIconAsync = chooseIconAsync ?? throw new ArgumentNullException(nameof(chooseIconAsync));
        this.remove = remove ?? throw new ArgumentNullException(nameof(remove));
        ChooseIconCommand = new AsyncRelayCommand(() => this.chooseIconAsync(this));
        RemoveCommand = new RelayCommand(() => this.remove(this));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id => draftEntry.Id;

    public string IconState
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(draftEntry.PendingIconSourcePath))
            {
                return $"будет обновлена: {Path.GetFileName(draftEntry.PendingIconSourcePath)}";
            }

            if (string.IsNullOrWhiteSpace(draftEntry.IconPath))
            {
                return "нет";
            }

            return File.Exists(draftEntry.IconPath)
                ? $"есть: {Path.GetFileName(draftEntry.IconPath)}"
                : "не найдена";
        }
    }

    public string IconPath => draftEntry.IconPath;

    public AsyncRelayCommand ChooseIconCommand { get; }

    public RelayCommand RemoveCommand { get; }

    public string DisplayName
    {
        get => draftEntry.DisplayName;
        set
        {
            if (draftEntry.DisplayName == value)
            {
                return;
            }

            draftEntry.DisplayName = value;
            OnPropertyChanged();
            changed();
        }
    }

    public string DefaultFolderName
    {
        get => draftEntry.DefaultFolderName;
        set
        {
            if (draftEntry.DefaultFolderName == value)
            {
                return;
            }

            draftEntry.DefaultFolderName = value;
            OnPropertyChanged();
            changed();
        }
    }

    public void RefreshIconState()
    {
        OnPropertyChanged(nameof(IconState));
        OnPropertyChanged(nameof(IconPath));
    }

    public bool IsEnabled
    {
        get => draftEntry.IsEnabled;
        set
        {
            if (draftEntry.IsEnabled == value)
            {
                return;
            }

            draftEntry.IsEnabled = value;
            OnPropertyChanged();
            changed();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
