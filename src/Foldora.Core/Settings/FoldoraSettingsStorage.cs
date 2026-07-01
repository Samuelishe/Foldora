using System.Text.Json;
using Foldora.Core.Menu;
using Foldora.Core.Storage;
using Foldora.Core.Validation;

namespace Foldora.Core.Settings;

/// <summary>
/// Хранит пользовательские настройки Foldora в JSON внутри AppData.
/// </summary>
public sealed class FoldoraSettingsStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly FoldoraDataPaths paths;

    public FoldoraSettingsStorage(FoldoraDataPaths paths)
    {
        this.paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(paths.RootDirectory);
        Directory.CreateDirectory(paths.IconsDirectory);
        Directory.CreateDirectory(paths.PreviewsDirectory);
        Directory.CreateDirectory(paths.PacksDirectory);

        if (!File.Exists(paths.SettingsFile))
        {
            await SaveAsync(new FoldoraSettings(), cancellationToken);
        }
    }

    public async Task<FoldoraSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        await EnsureCreatedAsync(cancellationToken);

        await using var stream = File.OpenRead(paths.SettingsFile);
        var settings = await JsonSerializer.DeserializeAsync<FoldoraSettings>(
            stream,
            JsonOptions,
            cancellationToken);

        return Normalize(settings ?? new FoldoraSettings());
    }

    public async Task SaveAsync(FoldoraSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Directory.CreateDirectory(paths.RootDirectory);

        await using var stream = new FileStream(
            paths.SettingsFile,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        await JsonSerializer.SerializeAsync(stream, Normalize(settings), JsonOptions, cancellationToken);
        await stream.WriteAsync("\n"u8.ToArray(), cancellationToken);
    }

    private static FoldoraSettings Normalize(FoldoraSettings settings)
    {
        var language = FoldoraLanguage.NormalizeOrDefault(settings.Language);
        var menu = NormalizeMenu(settings.CreateFolderMenu, language);

        return settings with
        {
            Language = language,
            CreateFolderMenu = menu
        };
    }

    private static FolderMenuSettings NormalizeMenu(FolderMenuSettings menu, string language)
    {
        var title = FolderMenuDefaultTitles.Normalize(menu.Title);
        var titleIsCustom = menu.TitleIsCustom || !FolderMenuDefaultTitles.IsKnownDefault(title);
        if (string.IsNullOrEmpty(title))
        {
            title = FolderMenuDefaultTitles.GetForLanguage(language);
            titleIsCustom = false;
        }

        foreach (var entry in menu.Entries)
        {
            entry.DefaultFolderName = FolderNameValidator.NormalizeOrDefault(entry.DefaultFolderName);
            entry.GroupName = GroupNameValidator.Normalize(entry.GroupName);
        }

        return new FolderMenuSettings
        {
            Title = titleIsCustom ? title : FolderMenuDefaultTitles.GetForLanguage(language),
            TitleIsCustom = titleIsCustom,
            Entries = menu.Entries
        };
    }
}
