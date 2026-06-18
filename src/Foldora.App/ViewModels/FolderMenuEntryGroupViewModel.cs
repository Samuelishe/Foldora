using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Foldora.App.ViewModels;

/// <summary>
/// Presentation-only секция пунктов меню для визуальной группировки WPF-редактора.
/// </summary>
public sealed class FolderMenuEntryGroupViewModel : INotifyPropertyChanged
{
    private readonly Action<string, string> renameGroup;
    private string title;
    private bool isDeleteConfirmationVisible;

    public FolderMenuEntryGroupViewModel(
        string groupName,
        string title,
        IEnumerable<FolderMenuEntryViewModel> entries,
        string addEntryText,
        string deleteGroupText,
        string confirmDeleteGroupText,
        string cancelText,
        bool isRootSection,
        Action<string> addEntryToGroup,
        Action<string> deleteGroup,
        Action<string, string> renameGroup)
    {
        GroupName = groupName;
        this.title = title;
        Entries = new ObservableCollection<FolderMenuEntryViewModel>(entries);
        AddEntryText = addEntryText;
        DeleteGroupText = deleteGroupText;
        ConfirmDeleteGroupText = confirmDeleteGroupText;
        CancelText = cancelText;
        IsRootSection = isRootSection;
        this.renameGroup = renameGroup ?? throw new ArgumentNullException(nameof(renameGroup));
        AddEntryCommand = new RelayCommand(() => addEntryToGroup(GroupName));
        DeleteGroupCommand = new RelayCommand(ShowDeleteConfirmation, () => CanDeleteGroup);
        ConfirmDeleteGroupCommand = new RelayCommand(() => deleteGroup(GroupName), () => CanDeleteGroup);
        CancelDeleteGroupCommand = new RelayCommand(HideDeleteConfirmation);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string GroupName { get; }

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
            OnPropertyChanged();
            if (!IsRootSection)
            {
                renameGroup(GroupName, value);
            }
        }
    }

    public ObservableCollection<FolderMenuEntryViewModel> Entries { get; }

    public bool IsRootSection { get; }

    public bool CanDeleteGroup => !IsRootSection;

    public bool IsDeleteConfirmationVisible
    {
        get => isDeleteConfirmationVisible;
        private set
        {
            if (isDeleteConfirmationVisible == value)
            {
                return;
            }

            isDeleteConfirmationVisible = value;
            OnPropertyChanged();
        }
    }

    public string AddEntryText { get; }

    public string DeleteGroupText { get; }

    public string ConfirmDeleteGroupText { get; }

    public string CancelText { get; }

    public RelayCommand AddEntryCommand { get; }

    public RelayCommand DeleteGroupCommand { get; }

    public RelayCommand ConfirmDeleteGroupCommand { get; }

    public RelayCommand CancelDeleteGroupCommand { get; }

    private void ShowDeleteConfirmation()
    {
        IsDeleteConfirmationVisible = true;
    }

    private void HideDeleteConfirmation()
    {
        IsDeleteConfirmationVisible = false;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
