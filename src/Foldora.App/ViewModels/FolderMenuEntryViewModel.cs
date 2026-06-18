using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Foldora.App.Services;
using Foldora.Core.Menu;

namespace Foldora.App.ViewModels;

/// <summary>
/// ViewModel draft-пункта меню для phase 1 WPF-редактора.
/// </summary>
public sealed class FolderMenuEntryViewModel : INotifyPropertyChanged
{
    private readonly FolderMenuDraftEntry draftEntry;
    private readonly IIconPreviewService iconPreviewService;
    private readonly Action changed;
    private readonly Action groupChanged;
    private readonly Action<FolderMenuEntryViewModel> editRequested;
    private readonly Func<FolderMenuEntryViewModel, Task> chooseIconAsync;
    private readonly Action<FolderMenuEntryViewModel> remove;
    private bool isEditing;

    public FolderMenuEntryViewModel(
        FolderMenuDraftEntry draftEntry,
        IIconPreviewService iconPreviewService,
        Action changed,
        Action groupChanged,
        Action<FolderMenuEntryViewModel> editRequested,
        Func<FolderMenuEntryViewModel, Task> chooseIconAsync,
        Action<FolderMenuEntryViewModel> remove)
    {
        this.draftEntry = draftEntry ?? throw new ArgumentNullException(nameof(draftEntry));
        this.iconPreviewService = iconPreviewService ?? throw new ArgumentNullException(nameof(iconPreviewService));
        this.changed = changed ?? throw new ArgumentNullException(nameof(changed));
        this.groupChanged = groupChanged ?? throw new ArgumentNullException(nameof(groupChanged));
        this.editRequested = editRequested ?? throw new ArgumentNullException(nameof(editRequested));
        this.chooseIconAsync = chooseIconAsync ?? throw new ArgumentNullException(nameof(chooseIconAsync));
        this.remove = remove ?? throw new ArgumentNullException(nameof(remove));
        EditCommand = new RelayCommand(() => this.editRequested(this));
        FinishEditingCommand = new RelayCommand(FinishEditing);
        ChooseIconCommand = new AsyncRelayCommand(() => this.chooseIconAsync(this));
        RemoveCommand = new RelayCommand(() => this.remove(this));
        RefreshIconPreview();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id => draftEntry.Id;

    public string IconState
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(draftEntry.PendingIconSourcePath))
            {
                if (!string.IsNullOrWhiteSpace(IconPreviewError))
                {
                    return $"preview не загружен: {Path.GetFileName(draftEntry.PendingIconSourcePath)}";
                }

                return $"будет обновлена: {Path.GetFileName(draftEntry.PendingIconSourcePath)}";
            }

            if (string.IsNullOrWhiteSpace(draftEntry.IconPath))
            {
                return "нет";
            }

            if (!File.Exists(draftEntry.IconPath))
            {
                return "не найдена";
            }

            return string.IsNullOrWhiteSpace(IconPreviewError)
                ? $"есть: {Path.GetFileName(draftEntry.IconPath)}"
                : "preview не загружен";
        }
    }

    public string IconPath => draftEntry.IconPath;

    public ImageSource? IconPreview { get; private set; }

    public bool HasIconPreview => IconPreview is not null;

    public string? IconPreviewError { get; private set; }

    public ObservableCollection<string> InlineErrors { get; } = [];

    public bool HasInlineErrors => InlineErrors.Count > 0;

    public bool IsEditing
    {
        get => isEditing;
        private set
        {
            if (isEditing == value)
            {
                return;
            }

            isEditing = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand EditCommand { get; }

    public RelayCommand FinishEditingCommand { get; }

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
            OnPropertyChanged(nameof(CompactTitle));
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
            OnPropertyChanged(nameof(CompactFolderName));
            changed();
        }
    }

    public string GroupName
    {
        get => draftEntry.GroupName;
        set
        {
            if (draftEntry.GroupName == value)
            {
                return;
            }

            draftEntry.GroupName = value;
            OnPropertyChanged();
            groupChanged();
        }
    }

    public string CompactTitle => string.IsNullOrWhiteSpace(DisplayName) ? "(без названия)" : DisplayName;

    public string CompactFolderName => string.IsNullOrWhiteSpace(DefaultFolderName) ? "Новая папка" : DefaultFolderName;

    public void RefreshIconState()
    {
        RefreshIconPreview();
        OnPropertyChanged(nameof(IconState));
        OnPropertyChanged(nameof(IconPath));
    }

    private void RefreshIconPreview()
    {
        var preview = iconPreviewService.LoadPreview(GetPreviewIconPath());
        IconPreview = preview.Image;
        IconPreviewError = preview.ErrorMessage;
        OnPropertyChanged(nameof(IconPreview));
        OnPropertyChanged(nameof(HasIconPreview));
        OnPropertyChanged(nameof(IconPreviewError));
    }

    private string? GetPreviewIconPath()
    {
        return string.IsNullOrWhiteSpace(draftEntry.PendingIconSourcePath)
            ? draftEntry.IconPath
            : draftEntry.PendingIconSourcePath;
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

    public void BeginEditing()
    {
        IsEditing = true;
    }

    public void CollapseEditing()
    {
        IsEditing = false;
    }

    public void AddInlineError(string message)
    {
        InlineErrors.Add(message);
        OnPropertyChanged(nameof(HasInlineErrors));
        BeginEditing();
    }

    public void ClearInlineErrors()
    {
        if (InlineErrors.Count == 0)
        {
            return;
        }

        InlineErrors.Clear();
        OnPropertyChanged(nameof(HasInlineErrors));
    }

    private void FinishEditing()
    {
        if (HasInlineErrors)
        {
            return;
        }

        CollapseEditing();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
