namespace Foldora.MenuHost;

internal interface IPlacementRetryDelay
{
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default);
}

internal sealed class PlacementRetryDelay : IPlacementRetryDelay
{
    public async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        await Task.Delay(delay, cancellationToken);
    }
}

internal sealed record DesktopPlacementRetryPolicy(int MaxAttempts, TimeSpan Delay)
{
    public static DesktopPlacementRetryPolicy Default { get; } = new(10, TimeSpan.FromMilliseconds(125));
}
