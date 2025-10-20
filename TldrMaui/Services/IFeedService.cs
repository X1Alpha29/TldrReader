using TldrMaui.Models;

namespace TldrMaui.Services;

public interface IFeedService
{
    Task<IReadOnlyList<FeedItem>> GetLatestAsync(FeedKind kind, CancellationToken ct = default);
    Task<IReadOnlyList<FeedItem>> LoadCacheAsync(FeedKind kind, CancellationToken ct = default);
    Task SaveCacheAsync(FeedKind kind, IReadOnlyList<FeedItem> items, CancellationToken ct = default);
}
