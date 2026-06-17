using Foldora.Core.Settings;
using Foldora.Core.Storage;
using Foldora.Core.Validation;

namespace Foldora.Core.Menu;

/// <summary>
/// Управляет пользовательскими пунктами меню создания папки.
/// </summary>
public sealed class FolderMenuService
{
    private readonly FoldoraSettingsStorage storage;
    private readonly FoldoraDataPaths paths;
    private readonly IconImportService iconImportService;
    private readonly FolderMenuSettingsValidator settingsValidator = new();

    public FolderMenuService(
        FoldoraSettingsStorage storage,
        FoldoraDataPaths paths,
        IconImportService? iconImportService = null)
    {
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        this.paths = paths ?? throw new ArgumentNullException(nameof(paths));
        this.iconImportService = iconImportService ?? new IconImportService();
    }

    public async Task<IReadOnlyList<FolderMenuEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        var settings = await storage.LoadAsync(cancellationToken);
        return settings.CreateFolderMenu.Entries
            .OrderBy(entry => entry.SortOrder)
            .ThenBy(entry => entry.DisplayName, StringComparer.CurrentCulture)
            .ToArray();
    }

    public async Task<FolderMenuEntry> AddAsync(
        string sourceIconPath,
        string? displayName,
        string? defaultFolderName = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await storage.LoadAsync(cancellationToken);
        var resolvedDisplayName = string.IsNullOrWhiteSpace(displayName)
            ? FolderMenuNameGenerator.GetNextName(settings.CreateFolderMenu.Entries)
            : DisplayNameValidator.Normalize(displayName);
        var resolvedDefaultFolderName = FolderNameValidator.NormalizeOrDefault(defaultFolderName);

        ValidateInputBeforeImport(resolvedDisplayName, resolvedDefaultFolderName, settings.CreateFolderMenu);

        var importedIcon = await iconImportService.ImportAsync(sourceIconPath, paths, cancellationToken);

        var entry = new FolderMenuEntry
        {
            Id = importedIcon.EntryId,
            DisplayName = resolvedDisplayName,
            DefaultFolderName = resolvedDefaultFolderName,
            IconPath = importedIcon.ImportedIconPath,
            SortOrder = GetNextSortOrder(settings.CreateFolderMenu.Entries),
            IsEnabled = true
        };

        settings.CreateFolderMenu.Entries.Add(entry);
        EnsureValid(settings.CreateFolderMenu);
        await storage.SaveAsync(settings, cancellationToken);
        return entry;
    }

    public async Task<FolderMenuEntry> RemoveAsync(
        string entryId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);

        var settings = await storage.LoadAsync(cancellationToken);
        var entry = settings.CreateFolderMenu.Entries.FirstOrDefault(
            item => string.Equals(item.Id, entryId, StringComparison.OrdinalIgnoreCase));

        if (entry is null)
        {
            throw new InvalidOperationException($"Menu entry was not found: {entryId}");
        }

        settings.CreateFolderMenu.Entries.Remove(entry);
        DeleteImportedIconIfOwned(entry);
        EnsureValid(settings.CreateFolderMenu);
        await storage.SaveAsync(settings, cancellationToken);
        return entry;
    }

    private void ValidateInputBeforeImport(
        string displayName,
        string defaultFolderName,
        FolderMenuSettings menuSettings)
    {
        var issues = new List<FolderMenuValidationIssue>();
        issues.AddRange(DisplayNameValidator.ValidateResolved(displayName).Issues);
        issues.AddRange(FolderNameValidator.Validate(defaultFolderName).Issues);

        var prospectiveSettings = new FolderMenuSettings();
        foreach (var existingEntry in menuSettings.Entries)
        {
            prospectiveSettings.Entries.Add(existingEntry);
        }

        prospectiveSettings.Entries.Add(new FolderMenuEntry
        {
            Id = "entry-validation-preview",
            DisplayName = displayName,
            DefaultFolderName = defaultFolderName,
            IconPath = "validation-preview.ico",
            SortOrder = GetNextSortOrder(menuSettings.Entries)
        });

        issues.AddRange(settingsValidator.Validate(prospectiveSettings).Issues);
        ThrowIfInvalid(new FolderMenuValidationResult(issues));
    }

    private void EnsureValid(FolderMenuSettings settings)
    {
        ThrowIfInvalid(settingsValidator.Validate(settings));
    }

    private static void ThrowIfInvalid(FolderMenuValidationResult result)
    {
        if (result.IsValid)
        {
            return;
        }

        throw new InvalidOperationException(result.Issues.First(issue => issue.Severity == FolderMenuValidationSeverity.Error).Message);
    }

    private void DeleteImportedIconIfOwned(FolderMenuEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.IconPath) || !File.Exists(entry.IconPath))
        {
            return;
        }

        var iconsRoot = Path.GetFullPath(paths.IconsDirectory);
        var iconPath = Path.GetFullPath(entry.IconPath);
        var expectedPath = Path.GetFullPath(Path.Combine(paths.IconsDirectory, $"{entry.Id}.ico"));

        if (!string.Equals(iconPath, expectedPath, StringComparison.OrdinalIgnoreCase)
            || !iconPath.StartsWith(iconsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        File.Delete(iconPath);
    }

    private static int GetNextSortOrder(IEnumerable<FolderMenuEntry> entries)
    {
        return entries.Any() ? entries.Max(entry => entry.SortOrder) + 1 : 0;
    }
}
