namespace CounterStats.UI.Elements;

public class SidebarBoxRow : Gtk.ListBoxRow
{
    [Gtk.Connect] private readonly Gtk.Label _titleLabel;
    [Gtk.Connect] private readonly Gtk.Image _icon;
    [Gtk.Connect] private readonly Gtk.ListBoxRow _root;
    private SidebarBoxRow(Gtk.Builder builder, string name) : base(new Gtk.Internal.ListBoxRowHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public SidebarBoxRow(MainApp app, string title, string icon, int id) : this(new Gtk.Builder("SidebarBoxRow.ui"), "_root")
    {
        _titleLabel.SetText(title);
        _icon.IconName = icon;
        OnNotify += (_, _) => { app.SetWindow(id); };
    }
}
