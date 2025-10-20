using TldrMaui.Views;

namespace TldrMaui;

public partial class App : Application
{
    public App(FeedPage page)
    {
        InitializeComponent();
        MainPage = new NavigationPage(page);
    }
}
