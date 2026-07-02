namespace Foldora.Imaging;

/// <summary>
/// Ошибка или предупреждение будущего image-to-ICO conversion pipeline.
/// </summary>
public sealed record IconConversionError(string Code, string Message);

