namespace CounterStats.UI;

using CounterStats.UI.Windows;
using CounterStats.UI.Elements;
using Gio;
using GObject;
using HarfBuzz;

public class MainApp : Adw.ApplicationWindow
{
    [Gtk.Connect] private readonly Gtk.Stack _stack;
    [Gtk.Connect] private readonly Adw.WindowTitle _windowTitle;
    [Gtk.Connect] private readonly Gtk.Button _showSidebarButton;
    [Gtk.Connect] private readonly Gtk.ListBox _listView;
    [Gtk.Connect] private readonly Adw.OverlaySplitView _splitView;  
    private ConfigurationManager _configurationManager;
    private UpdatesWindow? _updatesWindow;
    private StatsWindow? _statsWindow;
    private ProfileWindow? _profileWindow;
    private LeaderboardWindow? _leaderboardWindow;
    private InventoryWindow? _inventoryWindow;
    private MainApp(Gtk.Builder builder, string name) : base(new Adw.Internal.ApplicationWindowHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public MainApp(Adw.Application application) : this(new Gtk.Builder("MainApp.ui"), "_root")
    {
        Application = application;
        _configurationManager = new ConfigurationManager(this);
        application.ActiveWindow.WidthRequest = 900;
        application.OnShutdown += (_, _) => OnShutdownApp();
        _listView.OnRowActivated += (_, _) => { OnRowChange(); };
        _showSidebarButton.OnClicked += (_, _) => ToogleSplitView();
        //create actions
        CreateAction("About", (_, _) => { OnAboutAction(); });
        CreateAction("Quit", (_, _) => { Application.Quit(); }, ["<Ctrl>Q"]);
        CreateAction("Preferences", (_, _) => { OnPreferencesAction(); }, ["<Ctrl>comma"]);
        CreateAction("GetNews", (_, _) => { OnPreferencesAction(); }, ["<Ctrl>R"]);
        //windows
        _statsWindow = new StatsWindow(this, _configurationManager);
        _profileWindow = new ProfileWindow(this, _configurationManager);
        _updatesWindow = new UpdatesWindow(this, _configurationManager);
        _inventoryWindow = new InventoryWindow(this, _configurationManager);
        _leaderboardWindow = new LeaderboardWindow(this);

        //add windows to stack
        _stack.AddChild(_profileWindow);
        _stack.AddChild(_updatesWindow);
        _stack.AddChild(_inventoryWindow);
        _stack.AddChild(_statsWindow);
        _stack.AddChild(_leaderboardWindow);
        //select the row in the sidebar an set the window
        SetWindow(_configurationManager.DefaultWindow + 1);
        _listView.SelectRow(GetListBoxRowByID(_configurationManager.DefaultWindow));
    }
    private void OnAboutAction()
    {         
        var about = Adw.AboutDialog.New();
        about.ApplicationName = "Counter Stats";
        about.SetApplicationIcon("org.counterstats");
        about.DeveloperName = "El Bandito❦Mágico";
        about.Version = Globals.VERSION;
        about.Developers = ["El Bandito❦Mágico"];
        about.Copyright = "© 2025 El Bandito❦Mágico";
        about.AddLink("Source code at GitHub", "https://github.com/Piotrekkn/CounterStats");
        about.Present(this);
    }
    private void OnPreferencesAction()
    {
        PreferencesDialog? preferencesDialog = new PreferencesDialog(this, _configurationManager);
        preferencesDialog.Present(this);
    }
    private void OnShutdownApp()
    {
        if (_configurationManager.ClearCacheOnQuit)
        {
            _configurationManager.ClearCache();
        }
        Console.WriteLine("BYE BYE");
    }
    public void SetTitle(string title, string subtitle = "")
    {
        _windowTitle.SetTitle(title);
        _windowTitle.SetSubtitle(subtitle);
    }

    private void CreateAction(string name, SignalHandler<SimpleAction, SimpleAction.ActivateSignalArgs> callback, string[]? shortcuts = null)
    {
        var lowerName = name.ToLowerInvariant();
        var actionItem = SimpleAction.New(lowerName, null);
        actionItem.OnActivate += callback;
        Application.AddAction(actionItem);

        if (shortcuts is { Length: > 0 })
        {
            Application.SetAccelsForAction($"app.{lowerName}", shortcuts);
        }
    }

    private void ToogleSplitView()
    {
        _splitView.SetCollapsed(!_splitView.GetCollapsed());
    }

    private Gtk.ListBoxRow GetListBoxRowByID(int id)
    {
        Gtk.Widget listBoxRow = _listView.GetFirstChild();
        while (id > 0)
        {
            listBoxRow = listBoxRow.GetNextSibling();
            id--;
        }
        return (Gtk.ListBoxRow)listBoxRow;
    }
    private void OnRowChange()
    {
        Console.WriteLine(_listView.GetFocusChild().Name);
        if (_listView.GetFocusChild().Name == "profile-window")
            SetWindow(1);
        if (_listView.GetFocusChild().Name == "updates-window")
            SetWindow(2);
        if (_listView.GetFocusChild().Name == "inventory-window")
            SetWindow(3);
        if (_listView.GetFocusChild().Name == "leaderboard-window")
            SetWindow(4);
        if (_listView.GetFocusChild().Name == "stats-window")
            SetWindow(5);

    }
    public void SetWindow(int windowId)
    {
        Console.WriteLine($"Window {windowId}");
        switch (windowId)
        {
            case 1:
                _stack.SetFocusChild(_profileWindow);
                _stack.VisibleChild = _profileWindow;
                break;
            case 2:
                _stack.SetFocusChild(_updatesWindow);
                _stack.VisibleChild = _updatesWindow;
                break;
            case 3:
                _stack.SetFocusChild(_inventoryWindow);
                _stack.VisibleChild = _inventoryWindow;
                break;
            case 4:
                _stack.SetFocusChild(_leaderboardWindow);
                _stack.VisibleChild = _leaderboardWindow;
                break;
            case 5:
                _stack.SetFocusChild(_statsWindow);
                _stack.VisibleChild = _statsWindow;
                break;
            case 0:
            default:
                _stack.SetFocusChild(null);
                break;
        }

    }


}
