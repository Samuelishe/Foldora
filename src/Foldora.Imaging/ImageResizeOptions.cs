namespace Foldora.Imaging;

public sealed record ImageResizeOptions
{
    public ImageResizeFilter Filter { get; init; } = ImageResizeFilter.Lanczos3;
}
