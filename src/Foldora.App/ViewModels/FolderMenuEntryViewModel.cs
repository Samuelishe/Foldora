using System.ComponentModel;
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

    public FolderMenuEntryViewModel(FolderMenuDraftEntry draftEntry, Action changed)
    {
        this.draftEntry = draftEntry ?? throw new ArgumentNullException(nameof(draftEntry));
        this.changed = changed ?? throw new ArgumentNullException(nameof(changed));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id => draftEntry.Id;

    public string IconState => string.IsNullOrWhiteSpace(draftEntry.IconPath) ? "нет" : "есть";

    public string IconPath => draftEntry.IconPath;

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
