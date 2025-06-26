namespace CounterStats.UI.Windows;

public class FindPlayerWindow : Gtk.Box, IWindow
{
    [Gtk.Connect] private readonly Gtk.Button _finnishPageButton;
    [Gtk.Connect] private readonly Adw.StatusPage _findPlayerStatus;
    [Gtk.Connect] private readonly Gtk.Box _profileBox;
    [Gtk.Connect] private readonly Gtk.Entry _profileEntry;
    public string WindowName { get; }
    public string IconName { get; }
    MainApp _mainApp;
    ConfigurationManager _configuration;
    private FindPlayerWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }

    public FindPlayerWindow(MainApp mainApp, ConfigurationManager configuration, string windowName, string iconName) : this(new Gtk.Builder("FindPlayerWindow.ui"), "_root")
    {
        _configuration = configuration;
        WindowName = windowName;
        IconName = iconName;
        _mainApp = mainApp;
        OnRealize += (sender, e) => Refresh();
        OnMap += (_, _) => mainApp.SetTitle(WindowName);
        mainApp.SetTitle(WindowName);
        _finnishPageButton.OnClicked += (_, _) => { SetPlayerAsync(_profileEntry.GetText()); };
        _profileEntry.OnActivate += (_, _) => { SetPlayerAsync(_profileEntry.GetText()); };
    }

    public void CleanChildren()
    {
        _findPlayerStatus.SetVisible(true);
        _profileBox.SetVisible(false);
        Gtk.Widget toRemove = _profileBox.GetLastChild();
        while (toRemove != null)
        {
            _profileBox.Remove(toRemove);
            toRemove = _profileBox.GetLastChild();
        }
    }

    public void Refresh()
    {
        CleanChildren();
    }


    private async Task SetPlayerAsync(string steamid)
    {
        string id = await Globals.GetSteamID64(steamid, _configuration.ApiKey);
        ProfileWindow profileWindow = new ProfileWindow(_mainApp, _configuration, WindowName, IconName, id);
        _findPlayerStatus.SetVisible(false);
        _profileBox.SetVisible(true);
        _profileBox.Append(profileWindow);
    }

}
