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

        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
        await stream.WriteAsync("\n"u8.ToArray(), cancellationToken);
    }

    private static FoldoraSettings Normalize(FoldoraSettings settings)
    {
        foreach (var entry in settings.CreateFolderMenu.Entries)
        {
            entry.DefaultFolderName = FolderNameValidator.NormalizeOrDefault(entry.DefaultFolderName);
        }

        return settings with { Language = FoldoraLanguage.NormalizeOrDefault(settings.Language) };
    }
}
