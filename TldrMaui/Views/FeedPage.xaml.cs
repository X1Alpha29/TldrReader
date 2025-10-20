using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel; // Share, Clipboard
using TldrMaui.ViewModels;
using TldrMaui.Models;
using System.Linq;

namespace TldrMaui.Views;

public partial class FeedPage : ContentPage
{
    private FeedViewModel? _vm;

    public FeedPage(FeedViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        _vm = BindingContext as FeedViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is FeedViewModel vm)
        {
            // First-time load (or if user navigates back and list is empty)
            if (!vm.Groups.Any() && !vm.IsBusy)
            {
                // Tiny delay lets the page render before the spinner starts
                await Task.Delay(50);
                await vm.LoadAsync();
            }
        }
    }

    // Inline button Share
    private async void OnShareClicked(object sender, EventArgs e)
    {
        var item =
            (sender as Button)?.CommandParameter as FeedItem ??
            (sender as Element)?.BindingContext as FeedItem;

        if (item?.Link is string url && !string.IsNullOrWhiteSpace(url))
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = item.Title,
                Uri = url,
                Title = "Share TLDR link"
            });
        }
    }

    // Context menu Share
    private async void OnShareMenuClicked(object sender, EventArgs e)
    {
        var item = (sender as MenuFlyoutItem)?.CommandParameter as FeedItem;
        if (item?.Link is string url && !string.IsNullOrWhiteSpace(url))
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = item.Title,
                Uri = url,
                Title = "Share TLDR link"
            });
        }
    }

    // Inline or menu: Copy link
    private async void OnCopyLinkClicked(object sender, EventArgs e)
    {
        FeedItem? item = null;

        if (sender is Button b1)
            item = b1.CommandParameter as FeedItem ?? b1.BindingContext as FeedItem;
        else if (sender is MenuFlyoutItem m)
            item = m.CommandParameter as FeedItem ?? m.BindingContext as FeedItem;

        if (item?.Link is string url && !string.IsNullOrWhiteSpace(url))
        {
            await Clipboard.SetTextAsync(url);
            await DisplayAlert("Copied", "Link copied to clipboard", "OK");
        }
    }
    private async void OnMoreClicked(object sender, EventArgs e)
    {
        var item = (sender as Button)?.CommandParameter as TldrMaui.Models.FeedItem
                   ?? (sender as Element)?.BindingContext as TldrMaui.Models.FeedItem;
        if (item == null || string.IsNullOrWhiteSpace(item.Link))
            return;

        var choice = await DisplayActionSheet("Actions", "Cancel", null, "Open", "Share", "Copy link");
        switch (choice)
        {
            case "Open":
                await Launcher.Default.OpenAsync(item.Link);
                break;
            case "Share":
                await Share.RequestAsync(new ShareTextRequest
                {
                    Text = item.Title,
                    Uri = item.Link,
                    Title = "Share TLDR link"
                });
                break;
            case "Copy link":
                await Clipboard.SetTextAsync(item.Link);
                await DisplayAlert("Copied", "Link copied to clipboard", "OK");
                break;
        }
    }

}
