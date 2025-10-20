using System.ServiceModel.Syndication;
using System.Xml;
using System.Text.Json;
using Microsoft.Maui.Storage;
using TldrMaui.Models;

namespace TldrMaui.Services;

public sealed class FeedService(HttpClient http) : IFeedService
{
    // Map each feed to a URL. If any non-Tech URL fails, we’ll fall back to Tech.
    private static string GetFeedUrl(FeedKind kind) => kind switch
    {
        FeedKind.Tech => "https://tldr.tech/api/rss/tech",
        FeedKind.AI => "https://tldr.tech/api/rss/ai",
        FeedKind.Design => "https://tldr.tech/api/rss/design",
        FeedKind.Crypto => "https://tldr.tech/api/rss/crypto",
        _ => "https://tldr.tech/api/rss/tech"
    };


    private static string CachePathFor(FeedKind kind)
        => Path.Combine(FileSystem.AppDataDirectory, $"tldr_cache_{kind.ToString().ToLowerInvariant()}.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private sealed class CacheEnvelope
    {
        public DateTimeOffset SavedAt { get; set; }
        public List<FeedItem> Items { get; set; } = new();
    }

    public async Task<IReadOnlyList<FeedItem>> LoadCacheAsync(FeedKind kind, CancellationToken ct = default)
    {
        try
        {
            var path = CachePathFor(kind);
            if (!File.Exists(path)) return Array.Empty<FeedItem>();
            await using var fs = File.OpenRead(path);
            var env = await JsonSerializer.DeserializeAsync<CacheEnvelope>(fs, JsonOpts, ct);
            return env?.Items ?? new List<FeedItem>();
        }
        catch { return Array.Empty<FeedItem>(); }
    }

    public async Task SaveCacheAsync(FeedKind kind, IReadOnlyList<FeedItem> items, CancellationToken ct = default)
    {
        try
        {
            var path = CachePathFor(kind);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using var fs = File.Create(path);
            var env = new CacheEnvelope { SavedAt = DateTimeOffset.UtcNow, Items = items.ToList() };
            await JsonSerializer.SerializeAsync(fs, env, JsonOpts, ct);
        }
        catch { /* best-effort */ }
    }

    public async Task<IReadOnlyList<FeedItem>> GetLatestAsync(FeedKind kind, CancellationToken ct = default)
    {
        var url = GetFeedUrl(kind);

        using var response = await http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

        var feed = SyndicationFeed.Load(reader);
        if (feed == null) return Array.Empty<FeedItem>();

        return feed.Items
            .OrderByDescending(i => i.PublishDate)
            .Select(i => new FeedItem
            {
                Title = i.Title?.Text?.Trim() ?? "(no title)",
                Link = i.Links?.FirstOrDefault()?.Uri?.ToString() ?? "",
                Category = i.Categories.FirstOrDefault()?.Name ?? "Tech",
                Published = i.PublishDate
            })
            .ToList();
    }
}
