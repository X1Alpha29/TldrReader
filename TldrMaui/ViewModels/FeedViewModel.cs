using System.Collections.ObjectModel;
using System.Windows.Input;
using TldrMaui.Models;
using TldrMaui.Services;
using Microsoft.Maui.Devices;                 // HapticFeedback (if you use it)
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;
using Microsoft.Maui; // for AppTheme
using Microsoft.Maui.Controls;

namespace TldrMaui.ViewModels;

public class FeedViewModel : BindableObject
{
    private readonly IFeedService _feedService;
    private string _statusMessage = "";
    private CancellationTokenSource? _loadCts;
    private FeedKind? _lastRenderedFeed;

    // ---- Status banner ----
    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    private bool _isStatusVisible;
    public bool IsStatusVisible
    {
        get => _isStatusVisible;
        set { _isStatusVisible = value; OnPropertyChanged(); }
    }

    // ---- Selected feed + chip flags ----
    private FeedKind _selectedFeed = FeedKind.Tech;
    private FeedItem? _previewItem;
    public FeedItem? PreviewItem
    {
        get => _previewItem;
        set { _previewItem = value; OnPropertyChanged(); }
    }

    private bool _isPreviewOpen;
    public bool IsPreviewOpen
    {
        get => _isPreviewOpen;
        set { _isPreviewOpen = value; OnPropertyChanged(); }
    }
    public FeedKind SelectedFeed
    {
        get => _selectedFeed;
        set
        {
            if (_selectedFeed == value) return;
            _selectedFeed = value;
            OnPropertyChanged();
            UpdateSelectedFlags();   // keep chip highlights in sync
        }
    }


    public bool IsTechSelected { get; private set; }
    public bool IsAISelected { get; private set; }
    public bool IsDesignSelected { get; private set; }
    public bool IsCryptoSelected { get; private set; }
    public ICommand ToggleThemeCommand { get; }

    private void UpdateSelectedFlags()
    {
        IsTechSelected = SelectedFeed == FeedKind.Tech;
        IsAISelected = SelectedFeed == FeedKind.AI;
        IsDesignSelected = SelectedFeed == FeedKind.Design;
        IsCryptoSelected = SelectedFeed == FeedKind.Crypto;

        OnPropertyChanged(nameof(IsTechSelected));
        OnPropertyChanged(nameof(IsAISelected));
        OnPropertyChanged(nameof(IsDesignSelected));
        OnPropertyChanged(nameof(IsCryptoSelected));
    }

    private int _lastTotalItems = 0;

    // grouped source for the UI
    public ObservableCollection<FeedGroup> Groups { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    // specifically for RefreshView spinner (two-way)
    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set { _isRefreshing = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand SelectFeedCommand { get; }
    public ICommand OpenPreviewCommand { get; }
    public ICommand ClosePreviewCommand { get; }

    public FeedViewModel(IFeedService feedService)
    {
        _feedService = feedService;

        RefreshCommand = new Command(async () => await LoadAsync(fromPullToRefresh: true));
        OpenCommand = new Command<FeedItem>(async item => { if (!string.IsNullOrWhiteSpace(item?.Link)) await Launcher.Default.OpenAsync(item.Link); });

        SelectFeedCommand = new Command<FeedKind>(async kind =>
        {
            if (SelectedFeed == kind) return;
            SelectedFeed = kind;
            await LoadAsync();
        });

        // NEW: theme toggle
        ToggleThemeCommand = new Command(() =>
        {
            var app = Application.Current;
            if (app is null) return;

            // Flip between Light and Dark explicitly (avoid Unspecified so user’s choice “sticks”)
            app.UserAppTheme = app.UserAppTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
            // If you want the header title color to react immediately:
            OnPropertyChanged(nameof(SelectedFeed));
        });

        UpdateSelectedFlags();
    }

    public async Task LoadAsync(bool fromPullToRefresh = false)
    {
        // cancel any in-flight load
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var ct = _loadCts.Token;

        var feed = SelectedFeed; // capture at start

        try
        {
            if (!fromPullToRefresh) IsBusy = true;
            IsRefreshing = true;

            // 1) Show cache immediately when switching feeds or on first load
            var needCache = !fromPullToRefresh && (_lastRenderedFeed != feed || !Groups.Any());
            if (needCache)
            {
                var cached = await _feedService.LoadCacheAsync(feed, ct);
                if (!ct.IsCancellationRequested && cached.Count > 0 && SelectedFeed == feed)
                {
                    var cachedGroups = cached
                        .Select(i => new { Item = i, Day = i.Published.LocalDateTime.Date })
                        .GroupBy(x => x.Day)
                        .OrderByDescending(g => g.Key)
                        .Select(g => new FeedGroup(g.Key, g.Select(x => x.Item)));

                    Groups.Clear();
                    foreach (var g in cachedGroups) Groups.Add(g);

                    _lastRenderedFeed = feed;
                }
            }

            // 2) Fetch fresh network data
            var latest = await _feedService.GetLatestAsync(feed, ct);

            // Guard: only apply if still the same feed & not canceled
            if (ct.IsCancellationRequested || SelectedFeed != feed) return;

            var grouped = latest
                .Select(i => new { Item = i, Day = i.Published.LocalDateTime.Date })
                .GroupBy(x => x.Day)
                .OrderByDescending(g => g.Key)
                .Select(g => new FeedGroup(g.Key, g.Select(x => x.Item)));

            Groups.Clear();
            foreach (var g in grouped) Groups.Add(g);

            _lastRenderedFeed = feed;

            // 3) Save cache best-effort
            await _feedService.SaveCacheAsync(feed, latest, ct);

            // Optional: status banner text
            StatusMessage = latest.Count == 0 ? "No stories found" : "Updated items";
            IsStatusVisible = true;
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                MainThread.BeginInvokeOnMainThread(() => IsStatusVisible = false);
            });
        }
        catch (OperationCanceledException)
        {
            // ignored — a newer load took over
        }
        catch
        {
            StatusMessage = "Couldn’t refresh";
            IsStatusVisible = true;
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                MainThread.BeginInvokeOnMainThread(() => IsStatusVisible = false);
            });
        }
        finally
        {
            if (!ct.IsCancellationRequested)
            {
                IsRefreshing = false;
                if (!fromPullToRefresh) IsBusy = false;
            }
        }
    }
}
