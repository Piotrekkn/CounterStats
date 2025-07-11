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
    [Gtk.Connect] private readonly Gtk.Button _refreshButton;
    [Gtk.Connect] private readonly Gtk.ListBox _listView;
    [Gtk.Connect] private readonly Adw.OverlaySplitView _splitView;
    [Gtk.Connect] private readonly MenuButton _menuButton;
    [Gtk.Connect] private readonly MenuButton _menuButtonHeader;
    private ConfigurationManager _configurationManager;
    private List<IWindow> _windowList = new List<IWindow>();
    private int breakpointWidth = 1150;
    private int defaultWidth = 940;
    private int defaultHeight = 600;

    private int[] separatorPositions = new int[] { 6, };

    private MainApp(Gtk.Builder builder, string name) : base(new Adw.Internal.ApplicationWindowHandle(builder.GetPointer(name), true))
    {
        builder.Connect(this);
        Console.WriteLine(builder.GetPointer(name).ToString());
    }

    public MainApp(Adw.Application application) : this(new Gtk.Builder("MainApp.ui"), "_root")
    {
        Application = application;
        _configurationManager = new ConfigurationManager(this);
        application.ActiveWindow.WidthRequest = defaultWidth;
        application.ActiveWindow.HeightRequest = defaultHeight;
        application.OnShutdown += (_, _) => OnShutdownApp();
        _showSidebarButton.OnClicked += (_, _) => ToogleSplitView();
        _refreshButton.OnClicked += (_, _) => OnRefreshAction();
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
        _windowList.Add(new FindPlayerWindow(this, _configurationManager, "Find Player", "edit-find-symbolic"));
        //separators in sidebar
        _listView.SetHeaderFunc((row, _) => SetSeparator(row));
        //add windows to stack and sidebar     
        for (int i = 0; i < _windowList.Count; i++)
        {   //stack
            _stack.AddChild((Gtk.Widget)_windowList[i]);
            //sidebar          
            SidebarBoxRow sidebarBoxRow = new SidebarBoxRow(this, _windowList[i].WindowName, _windowList[i].IconName, i);
            _listView.Append(sidebarBoxRow);
        }
        _listView.SelectRow(GetListBoxRowByID(_configurationManager.DefaultWindow));
        SetWindow(_configurationManager.DefaultWindow);
        if (_configurationManager.firstRun)
        {
            _stack.SetVisible(false);
            SetTitle("", "");
        }
        //Sidebar
        if (_configurationManager.HideSidebar)
        {
            _splitView.SetCollapsed(true);
            _menuButtonHeader.SetVisible(true);
            _menuButton.SetVisible(false);
        }
        //break point
        Adw.Breakpoint breakpoint = new Adw.Breakpoint();
        this.AddBreakpoint(breakpoint);
        breakpoint.SetCondition(Adw.BreakpointCondition.Parse("max-width:" + breakpointWidth.ToString()));
        breakpoint.OnApply += (_, _) =>
        {
            _splitView.SetCollapsed(true);
            _menuButtonHeader.SetVisible(true);
            _menuButton.SetVisible(false);
        };
        breakpoint.OnUnapply += (_, _) =>
        {
            if (!_configurationManager.HideSidebar)
            {
                _splitView.SetCollapsed(false);
                _menuButtonHeader.SetVisible(false);
                _menuButton.SetVisible(true);
            }
        };
        //theme switcher
        SetTheme();
        PopoverMenu menuSidebar = (PopoverMenu)_menuButton.GetPopover();
        PopoverMenu menuHeader = (PopoverMenu)_menuButtonHeader.GetPopover();
        menuSidebar.AddChild(new ThemeSelector(_configurationManager), "ThemeSelector");
        menuHeader.AddChild(new ThemeSelector(_configurationManager), "ThemeSelector");
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
        PreferencesDialog? preferencesDialog = new PreferencesDialog(_configurationManager, list.ToArray());
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

    public void ReloadWindow()
    {
        _stack.SetVisible(true);
        OnRefreshAction();
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
        if (_splitView.Collapsed == true && (this.GetWidth() <= breakpointWidth || _configurationManager.HideSidebar))
        {
            _splitView.SetShowSidebar(true);
        }
        else
        {
            _splitView.SetCollapsed(!_splitView.GetCollapsed());
            _menuButtonHeader.SetVisible(_splitView.GetCollapsed());
            _menuButton.SetVisible(!_splitView.GetCollapsed());
        }
    }

    private void SetSeparator(ListBoxRow row)
    {
        foreach (int i in separatorPositions)
        {
            if (row.GetIndex() == i)
            {
                var separator = Gtk.Separator.New(Gtk.Orientation.Horizontal);
                separator.AddCssClass("separator");
                row.SetHeader(separator);
                return;
            }
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

    private void SetTheme()
    {
        switch (_configurationManager.CurrentTheme)
        {
            case 1:
                Adw.StyleManager.GetDefault().SetColorScheme(Adw.ColorScheme.ForceLight);
                break;
            case 2:
                Adw.StyleManager.GetDefault().SetColorScheme(Adw.ColorScheme.ForceDark);
                break;
            case 0:
            default:
                Adw.StyleManager.GetDefault().SetColorScheme(Adw.ColorScheme.Default);
                break;
        }
    }
}
