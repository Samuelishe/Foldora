using System.Collections.ObjectModel;

namespace Foldora.App.ViewModels;

/// <summary>
/// Presentation-only секция пунктов меню для визуальной группировки WPF-редактора.
/// </summary>
public sealed class FolderMenuEntryGroupViewModel
{
    public FolderMenuEntryGroupViewModel(
        string groupName,
        string title,
        IEnumerable<FolderMenuEntryViewModel> entries,
        string addEntryText,
        Action<string> addEntryToGroup)
    {
        GroupName = groupName;
        Title = title;
        Entries = new ObservableCollection<FolderMenuEntryViewModel>(entries);
        AddEntryText = addEntryText;
        AddEntryCommand = new RelayCommand(() => addEntryToGroup(GroupName));
    }

    public string GroupName { get; }

    public string Title { get; }

    public ObservableCollection<FolderMenuEntryViewModel> Entries { get; }

    public string AddEntryText { get; }

    public RelayCommand AddEntryCommand { get; }
}
