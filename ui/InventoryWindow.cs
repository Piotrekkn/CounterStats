namespace ui;

using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using GdkPixbuf;
using GLib;
using Newtonsoft.Json.Linq;

public class InventoryWindow : Gtk.Box
{
    [Gtk.Connect] private readonly Gtk.Box inventory_window;
    [Gtk.Connect] private readonly Gtk.Box inventory_box;

    private string title = "";
    private string subtitle = "";
    private ConfigurationManager configuration;
    private string imageUrl = "https://community.cloudflare.steamstatic.com/economy/image/";
    private string baseURLstart = "https://steamcommunity.com/inventory/";
    private string baseURLend = "/730/2";
    private string baseURL;
    MainWindow mainWindow;
    private InventoryWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public InventoryWindow(MainWindow mainWindow, ConfigurationManager configuration) : this(new Gtk.Builder("InventoryWindow.ui"), "inventory_window")
    {
        this.mainWindow = mainWindow;
        this.configuration = configuration;
        baseURL = baseURLstart + configuration.SteamProfile + baseURLend;
        OnMap += (_, _) => mainWindow.SetTitle(title, subtitle);
        SetTitle("Inventory");
        OnRealize += (sender, e) => Fetch();
    }
    private void SetTitle(string title, string subtitle = "")
    {
        this.title = title;
        this.subtitle = subtitle;
        mainWindow.SetTitle(title, subtitle);
    }
    private void CleanChildren()
    {
        Gtk.Widget toRemove = inventory_box.GetLastChild();
        while (toRemove != null)
        {
            inventory_box.Remove(toRemove);
            toRemove = inventory_box.GetLastChild();
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
        inventory_box.Append(spinner);
    }
    private void Fetch()
    {
        CleanChildren();
        SetLoadingScreen();
        FetchData();
    }

    private void SetData(string data)
    {
        CleanChildren();
        JObject obj = JObject.Parse(data);
        JToken items = obj.SelectToken("$.descriptions");
        Gtk.FlowBox flowBox = new Gtk.FlowBox();
        flowBox.SetRowSpacing(10);
        flowBox.SetColumnSpacing(10);
        flowBox.SetMarginStart(12);
        flowBox.SetMarginBottom(12);
        flowBox.SetMarginEnd(12);
        flowBox.SetMarginTop(8);
        int limit = configuration.ItemsNumber;
        foreach (JToken item in items)
        {
            var namee = item.SelectToken("$.market_name");
            if (namee != null)
            {
                Console.WriteLine(item.ToString());
                var type = item.SelectToken("$.type");
                var imageUrlName = item.SelectToken("$.icon_url");
                var color = item.SelectToken("$.name_color");
                //get rarity color and item type
                var tags = item.SelectToken("$.tags");
                var itemTag = type;
                foreach (JToken tag in tags)
                {
                    if (tag.SelectToken("$.category").ToString() == "Rarity")
                    {
                        color = tag.SelectToken("$.color");
                    }
                    //get weapon type or item type
                    if (tag.SelectToken("$.category").ToString() == "Weapon")
                    {
                        itemTag = tag.SelectToken("$.localized_tag_name");
                    }
                    else if (tag.SelectToken("$.category").ToString() == "Type")
                    {
                        itemTag = tag.SelectToken("$.localized_tag_name");
                    }
                }

                flowBox.Append(itemBox(namee.ToString(), type.ToString(), imageUrl + imageUrlName.ToString() + "/?allow_animated=0", color.ToString(), itemTag.ToString()));
                limit--;
                if (limit == 0)
                {
                    break;
                }
            }
        }
        inventory_box.Append(flowBox);
    }

    private async Task SetItemImage(Gtk.Image image, string url)
    {
        using (var webClient = new WebClient())
        {

            byte[] imageBytes = await webClient.DownloadDataTaskAsync(url);
            Pixbuf pixbuf = FromBytes(imageBytes);
            image.SetFromPixbuf(pixbuf);
        }

    }

    private Gtk.Box itemBox(string name, string type, string imageUrl, string color, string itemTag)
    {
        Gtk.Box box = new Gtk.Box();
        box.SetCssClasses(["card"]);
        box.WidthRequest = 200;
        box.HeightRequest = 300;
        box.SetOrientation(Gtk.Orientation.Vertical);
        Gtk.Image image = new Gtk.Image();
        image.SetIconSize(Gtk.IconSize.Large);
        image.SetVexpand(true);
        image.SetHexpand(true);
        image.SetMarginTop(6);
        image.SetMarginEnd(2);
        image.SetMarginStart(2);
        Gtk.Label labelName = new Gtk.Label();
        Gtk.Label labelTag = new Gtk.Label();
        Gtk.Label labelType = new Gtk.Label();
        labelName.SetLabel(name);
        //color name label
        if (color != null)
        {
            labelName.SetUseMarkup(true);
            labelName.SetMarkup("<b><span size=\"large\" color=\"#" + color + "\">" + name.Replace("&", "&amp;") + "</span></b>");
        }
        else
        {
            labelName.SetMarkup("<b><span size=\"large\">" + name.Replace("&", "&amp;") + "</span></b>");
        }
        labelName.SetMarginBottom(6);
        labelTag.SetLabel(itemTag);
        labelTag.SetMarginBottom(6);
        labelType.SetLabel(type);
        labelType.SetMarginBottom(8);

        box.Append(image);
        box.Append(labelTag);
        box.Append(labelName);        
        box.Append(labelType);
        SetItemImage(image, imageUrl);
        return box;
    }
    private static Pixbuf FromBytes(byte[] data)
    {
        using var bytes = Bytes.New(data);
        var loader = PixbufLoader.New();
        loader.WriteBytes(bytes);
        loader.Close();

        return loader.GetPixbuf() ?? throw new Exception("No Pixbuf created.");
    }
    private async void FetchData()
    {

        try
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(baseURL))
                {
                    res.EnsureSuccessStatusCode();
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        if (data != null)
                        {

                            SetData(data);
                        }
                        else
                        {
                            //ALERT?
                        }
                    }
                }
            }

        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}
