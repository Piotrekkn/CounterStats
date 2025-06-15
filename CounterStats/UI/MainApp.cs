namespace CounterStats.UI;

using CounterStats.UI.Windows;
using CounterStats.UI.Elements;
using Gio;
using GObject;
using Gtk;

public class MainApp : Adw.ApplicationWindow
{
    [Gtk.Connect] private readonly Gtk.Stack _stack;
    [Gtk.Connect] private readonly Adw.WindowTitle _windowTitle;
    [Gtk.Connect] private readonly Gtk.Button _showSidebarButton;
    [Gtk.Connect] private readonly Gtk.ListBox _listView;
    [Gtk.Connect] private readonly Adw.OverlaySplitView _splitView;
    private Adw.Breakpoint _breakpoint;
    private ConfigurationManager _configurationManager;
    private List<IWindow> _windowList = new List<IWindow>();
    private int breakpointWidth = 1150;

    private MainApp(Gtk.Builder builder, string name) : base(new Adw.Internal.ApplicationWindowHandle(builder.GetPointer(name), true))
    {
        builder.Connect(this);
        Console.WriteLine(builder.GetPointer(name).ToString());
    }
    public MainApp(Adw.Application application) : this(new Gtk.Builder("MainApp.ui"), "_root")
    {
        Application = application;
        _configurationManager = new ConfigurationManager(this);
        application.ActiveWindow.WidthRequest = 940;
        application.OnShutdown += (_, _) => OnShutdownApp();
        _showSidebarButton.OnClicked += (_, _) => ToogleSplitView();
        //create actions
        CreateAction("About", (_, _) => { OnAboutAction(); });
        CreateAction("Quit", (_, _) => { Application.Quit(); }, ["<Ctrl>Q"]);
        CreateAction("Preferences", (_, _) => { OnPreferencesAction(); }, ["<Ctrl>comma"]);
        CreateAction("Refresh", (_, _) => { OnRefreshAction(); }, ["<Ctrl>R"]);
        //windows        
        _windowList.Add(new ProfileWindow(this, _configurationManager, "Your Profile", "avatar-default-symbolic"));
        _windowList.Add(new UpdatesWindow(this, _configurationManager, "Game Updates", "software-update-available-symbolic"));
        _windowList.Add(new InventoryWindow(this, _configurationManager, "Inventory", "package-x-generic-symbolic"));
        _windowList.Add(new LeaderboardWindow(this, "Leaderboards", "applications-games-symbolic"));
        _windowList.Add(new StatsWindow(this, _configurationManager, "Player Statistics", "view-list-symbolic"));
        _windowList.Add(new StatusWindow(this, _configurationManager, "Server Status", "network-workgroup-symbolic"));

        //add windows to stack and sidebar
        for (int i = 0; i < _windowList.Count; i++)
        {   //stack
            _stack.AddChild((Gtk.Widget)_windowList[i]);
            //sidebar
            SidebarBoxRow sidebarBoxRow = new SidebarBoxRow(this, _windowList[i].WindowName, _windowList[i].IconName, i);
            _listView.Append(sidebarBoxRow);
        }
        SetWindow(_configurationManager.DefaultWindow);
        _listView.SelectRow(GetListBoxRowByID(_configurationManager.DefaultWindow));
        //Sidebar
        if (_configurationManager.HideSidebar)
        {
            _splitView.SetCollapsed(true);
        }
        //break point
        Adw.Breakpoint breakpoint = new Adw.Breakpoint();
        this.AddBreakpoint(breakpoint);
        breakpoint.SetCondition(Adw.BreakpointCondition.Parse("max-width:" + breakpointWidth.ToString()));
        breakpoint.OnApply += (_, _) => { _splitView.SetCollapsed(true); };
        breakpoint.OnUnapply += (_, _) =>
        {
            if (!_configurationManager.HideSidebar)
            { _splitView.SetCollapsed(false); }
        };
    }
    private void OnAboutAction()
    {
        var about = Adw.AboutDialog.New();
        about.ApplicationName = "Counter Stats";
        about.SetApplicationIcon("org.counterstats.CounterStats");
        about.DeveloperName = "El Bandito❦Mágico";
        about.Version = Globals.VERSION;
        about.Developers = ["El Bandito❦Mágico"];
        about.Copyright = "© 2025 El Bandito❦Mágico";
        about.AddLink("Source code at GitHub", "https://github.com/Piotrekkn/CounterStats");
        about.Present(this);
    }
    private void OnPreferencesAction()
    {
        List<string> list = new List<string>();
        foreach (var item in _windowList)
        {
            list.Add(item.WindowName);
        }
        PreferencesDialog? preferencesDialog = new PreferencesDialog(this, _configurationManager, list.ToArray());
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
    private void OnRefreshAction()
    {
        IWindow window = (IWindow)_stack.GetVisibleChild();
        if (window != null)
        {
            window.Refresh();
        }
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
        if (_splitView.Collapsed == true && this.GetWidth() <= breakpointWidth)
        {
            _splitView.SetShowSidebar(true);
        }
        else
        {
            _splitView.SetCollapsed(!_splitView.GetCollapsed());
        }
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

    public void SetWindow(int windowId)
    {
        //check if valid window
        if (windowId < 0 || windowId >= _windowList.Count)
        {
            //check if there is at least one window
            if (_windowList.Count != 0)
            {
                windowId = 0;
            }
            else //there are no windows to choose from
            {
                return;
            }
        }
        _stack.SetFocusChild((Gtk.Widget)_windowList[windowId]);
        _stack.VisibleChild = (Gtk.Widget)_windowList[windowId];
    }
}
