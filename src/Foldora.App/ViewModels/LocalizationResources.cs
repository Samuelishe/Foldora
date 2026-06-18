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
    public string MenuSettings => Get();
    public string MenuTitle => Get();
    public string MenuEntries => Get();
    public string AddEntry => Get();
    public string EmptyTitle => Get();
    public string EmptyDescription => Get();
    public string EntryDisplayName => Get();
    public string EntryFolderName => Get();
    public string EntryGroupName => Get();
    public string EntryGroupHelp => Get();
    public string EntryEnabled => Get();
    public string EntryIcon => Get();
    public string ChooseIcon => Get();
    public string Delete => Get();
    public string ExplorerIntegration => Get();
    public string StatusLabel => Get();
    public string DryRun => Get();
    public string RegisterExplorer => Get();
    public string UnregisterExplorer => Get();
    public string TechnicalDetails => Get();
    public string DangerZone => Get();
    public string ResetDescription => Get();
    public string ResetIconNote => Get();
    public string ResetConfirm => Get();
    public string ResetMenu => Get();
    public string UnsavedChanges => Get();
    public string Reload => Get();
    public string Save => Get();
    public string Settings => Get();
    public string Minimize => Get();
    public string Maximize => Get();
    public string Close => Get();

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
