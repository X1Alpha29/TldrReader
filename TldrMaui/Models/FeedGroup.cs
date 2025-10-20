using System.Collections.ObjectModel;

namespace TldrMaui.Models;

public class FeedGroup : ObservableCollection<FeedItem>
{
    public string Title { get; }
    public DateTime Date { get; }

    public FeedGroup(DateTime date, IEnumerable<FeedItem> items) : base(items)
    {
        Date = date.Date;
        Title = Date.ToString("dddd, dd MMM yyyy");
    }
}
