namespace CounterStats.UI.Windows;

public class FindPlayerWindow : Gtk.Box, IWindow
{
    public string WindowName { get; }
    public string IconName { get; }

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
        OnRealize += (sender, e) => Refresh();
        OnMap += (_, _) => mainApp.SetTitle(WindowName);
        mainApp.SetTitle(WindowName);
    }

    public void CleanChildren()
    {
    }

    public void Refresh()
    {
        CleanChildren();
    }
}
