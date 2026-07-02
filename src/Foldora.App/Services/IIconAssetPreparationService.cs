namespace Foldora.App.Services;

/// <summary>
/// Подготавливает выбранный icon/image-файл для использования в draft entry.
/// </summary>
public interface IIconAssetPreparationService
{
    Task<IconAssetPreparationResult> PrepareAsync(string selectedFilePath, CancellationToken cancellationToken = default);
}
