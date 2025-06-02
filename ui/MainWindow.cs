namespace ui;
using Gio;
using GObject;

public class MainWindow : Adw.ApplicationWindow
{
    [Gtk.Connect] private readonly Gtk.Stack stack;
    [Gtk.Connect] private readonly Adw.WindowTitle window_title;
    [Gtk.Connect] private readonly Gtk.Button show_sidebar_button;
    [Gtk.Connect] private readonly Gtk.ListBox listview;
    [Gtk.Connect] private readonly Adw.OverlaySplitView split_view;
    [Gtk.Connect] private readonly Gtk.ListBoxRow profile_box_row;
    private ConfigurationManager configurationManager;
    private UpdatesWindow? updatesWindow;
    private StatsWindow? statsWindow;
    private ProfileWindow? profileWindow;
    private LeaderboardWindow? leaderboardWindow;
    private MainWindow(Gtk.Builder builder, string name) : base(new Adw.Internal.ApplicationWindowHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public MainWindow(Adw.Application application) : this(new Gtk.Builder("MainWindow.ui"), "main_window")
    {
        this.Application = application;
        configurationManager = new ConfigurationManager(this);
        application.OnShutdown += (_, _) => OnShutdownApp();
        application.ActiveWindow.WidthRequest = 900;
        listview.SelectRow(profile_box_row);
        listview.OnRowActivated += (_, _) => { OnRowChange(); };
        show_sidebar_button.OnClicked += (_, _) => ToogleSplitView();
        //creat actions
        CreateAction("About", (_, _) => { OnAboutAction(); });
        CreateAction("Quit", (_, _) => { this.Application.Quit(); }, ["<Ctrl>Q"]);
        CreateAction("Preferences", (_, _) => { OnPreferencesAction(); }, ["<Ctrl>comma"]);
        CreateAction("GetNews", (_, _) => { OnPreferencesAction(); }, ["<Ctrl>R"]);
        //windows
        statsWindow = new StatsWindow(this, configurationManager);
        profileWindow = new ProfileWindow(this, configurationManager);
        updatesWindow = new UpdatesWindow(this, configurationManager);
        leaderboardWindow = new LeaderboardWindow(this);
        //add windows to stack
        stack.AddChild(profileWindow);
        stack.AddChild(updatesWindow);
        stack.AddChild(statsWindow);
        stack.AddChild(leaderboardWindow);  
    }
    private void OnAboutAction()
    {
        var about = Adw.AboutWindow.New();
        about.TransientFor = this.Application.ActiveWindow;
        about.ApplicationName = "Counter Stats";
        about.ApplicationIcon = "com.counterststs";
        about.DeveloperName = "El Bandito❦Mágico";
        about.Version = VERSION;
        about.Developers = ["El Bandito❦Mágico"];
        about.Copyright = "© 2025 El Bandito❦Mágico";
        about.AddLink("Source code at GitHub", "https://github.com/Piotrekkn/CounterStats");
        about.Present();
    }
    private void OnPreferencesAction()
    {
        PreferencesDialog? preferencesDialog = new PreferencesDialog(this, configurationManager);
        preferencesDialog.Present(this);
    }
    private void OnShutdownApp()
    {
        if (configurationManager.ClearCacheOnQuit)
        {
            configurationManager.ClearCache();
        }
        Console.WriteLine("BYE BYE");
    }
    public void SetTitle(string title, string subtitle = "")
    {
        window_title.SetTitle(title);
        window_title.SetSubtitle(subtitle);
    }

    private void CreateAction(string name, SignalHandler<SimpleAction, SimpleAction.ActivateSignalArgs> callback, string[]? shortcuts = null)
    {
        var lowerName = name.ToLowerInvariant();
        var actionItem = SimpleAction.New(lowerName, null);
        actionItem.OnActivate += callback;
        this.Application.AddAction(actionItem);

        if (shortcuts is { Length: > 0 })
        {
            this.Application.SetAccelsForAction($"app.{lowerName}", shortcuts);
        }
    }

    private void ToogleSplitView()
    {
        split_view.SetCollapsed(!split_view.GetCollapsed());
    }
    private void OnRowChange()
    {
        Console.WriteLine(listview.GetFocusChild().Name);
        if (listview.GetFocusChild().Name == "profile-window")
            SetWindow(1);
        if (listview.GetFocusChild().Name == "stats-window")
            SetWindow(2);
        if (listview.GetFocusChild().Name == "updates-window")
            SetWindow(3);
        if (listview.GetFocusChild().Name == "leaderboard-window")
            SetWindow(4);
    }
    public void SetWindow(int windowId)
    {
        switch (windowId)
        {
            case 1:
                stack.SetFocusChild(profileWindow);
                stack.VisibleChild = profileWindow;                
                break;
            case 2:
                stack.SetFocusChild(statsWindow);
                stack.VisibleChild = statsWindow;               
                break;
            case 3:
                stack.SetFocusChild(updatesWindow);
                stack.VisibleChild = updatesWindow;              
                break;
            case 4:
                stack.SetFocusChild(leaderboardWindow);
                stack.VisibleChild = leaderboardWindow;                
                break;
            case 0:
            default:
                stack.SetFocusChild(null);
                break;
        }

    }


}
