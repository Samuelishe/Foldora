namespace Foldora.Core.Storage;

/// <summary>
/// Стандартные пути пользовательских данных Foldora в AppData.
/// </summary>
public sealed class FoldoraDataPaths
{
    public FoldoraDataPaths(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        RootDirectory = rootDirectory;
        SettingsFile = Path.Combine(rootDirectory, "settings.json");
        PacksDirectory = Path.Combine(rootDirectory, "packs");
        IconsDirectory = Path.Combine(rootDirectory, "icons");
        PreviewsDirectory = Path.Combine(rootDirectory, "previews");
    }

    public string RootDirectory { get; }

    public string SettingsFile { get; }

    public string PacksDirectory { get; }

    public string IconsDirectory { get; }

    public string PreviewsDirectory { get; }

    public static FoldoraDataPaths CreateDefault()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return new FoldoraDataPaths(Path.Combine(appData, "Foldora"));
    }
}
