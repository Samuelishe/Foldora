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

    public FoldoraDataPaths Paths => paths;

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        EnsureDataDirectories();

        if (!File.Exists(paths.SettingsFile))
        {
            await SaveAsync(new FoldoraSettings(), cancellationToken);
        }
    }

    public async Task<FoldoraSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        await EnsureCreatedAsync(cancellationToken);

        var result = await LoadWithLanguageMetadataAsync(createSettingsIfMissing: true, cancellationToken);
        return result.Settings;
    }

    public Task<FoldoraSettingsLoadResult> LoadWithLanguageMetadataAsync(CancellationToken cancellationToken = default)
    {
        return LoadWithLanguageMetadataAsync(createSettingsIfMissing: true, cancellationToken);
    }

    public async Task<FoldoraSettingsLoadResult> LoadWithLanguageMetadataAsync(
        bool createSettingsIfMissing,
        CancellationToken cancellationToken = default)
    {
        EnsureDataDirectories();

        if (!File.Exists(paths.SettingsFile))
        {
            if (createSettingsIfMissing)
            {
                await SaveAsync(new FoldoraSettings(), cancellationToken);
            }
            else
            {
                return new FoldoraSettingsLoadResult(
                    Normalize(new FoldoraSettings()),
                    LanguageWasPersisted: false,
                    LanguageWasSupported: false);
            }
        }

        var languageState = await ReadLanguageStateAsync(cancellationToken);

        await using var stream = File.OpenRead(paths.SettingsFile);
        var settings = await JsonSerializer.DeserializeAsync<FoldoraSettings>(
            stream,
            JsonOptions,
            cancellationToken);

        return new FoldoraSettingsLoadResult(
            Normalize(settings ?? new FoldoraSettings()),
            languageState.WasPersisted,
            languageState.WasSupported);
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

    private void EnsureDataDirectories()
    {
        Directory.CreateDirectory(paths.RootDirectory);
        Directory.CreateDirectory(paths.IconsDirectory);
        Directory.CreateDirectory(paths.PreviewsDirectory);
        Directory.CreateDirectory(paths.PacksDirectory);
    }

    private async Task<(bool WasPersisted, bool WasSupported)> ReadLanguageStateAsync(CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(paths.SettingsFile);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("language", out var languageElement)
            || languageElement.ValueKind != JsonValueKind.String)
        {
            return (false, false);
        }

        var language = languageElement.GetString();
        return (true, FoldoraLanguage.IsSupported(language));
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
