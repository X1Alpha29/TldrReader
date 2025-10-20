namespace TldrMaui.Models;

public class FeedItem
{
    public string Title { get; set; } = "";
    public string Link { get; set; } = "";
    public string Category { get; set; } = "";
    public DateTimeOffset Published { get; set; }
}
