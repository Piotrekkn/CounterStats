namespace ui;

using Newtonsoft.Json.Linq;

public class InventoryWindow : Gtk.Box
{
    [Gtk.Connect] private readonly Gtk.Box inventory_window;

    private string title = "";
    private string subtitle = "";
    MainWindow mainWindow;
    private InventoryWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public InventoryWindow(MainWindow mainWindow) : this(new Gtk.Builder("InventoryWindow.ui"), "inventory_window")
    {
        this.mainWindow = mainWindow;
        OnMap += (_, _) => mainWindow.SetTitle(title, subtitle);
        SetTitle("Inventory");
        //  OnRealize += (sender, e) => Fetch();



    }
    private void SetTitle(string title, string subtitle = "")
    {
        this.title = title;
        this.subtitle = subtitle;
        mainWindow.SetTitle(title, subtitle);
    }
    private void CleanChildren()
    {
        Gtk.Widget toRemove = inventory_window.GetLastChild();
        while (toRemove != null)
        {
            inventory_window.Remove(toRemove);
            toRemove = inventory_window.GetLastChild();
        }
    }
    private void SetLoadingScreen()
    {
        Gtk.Spinner spinner = new Gtk.Spinner();
        spinner.SetHexpand(true);
        spinner.SetVexpand(true);
        spinner.SetHalign(Gtk.Align.Center);
        spinner.SetValign(Gtk.Align.Center);
        spinner.SetSpinning(true);
        spinner.WidthRequest = 40;
        spinner.HeightRequest = 40;
        inventory_window.Append(spinner);
    }
    private void Fetch(int regionId = 0)
    {
        CleanChildren();
        SetLoadingScreen();
        //FetchData(regionId);
    }

    private void SetData(string data)
    {
        CleanChildren();

    }

}
