namespace CounterStats.UI.Windows;

using CounterStats.UI.Elements;
using Newtonsoft.Json.Linq;

public class InventoryWindow : Gtk.Box
{
    [Gtk.Connect] private readonly Gtk.Box inventory_box;
    [Gtk.Connect] private readonly Gtk.Box header_box;
    [Gtk.Connect] private readonly Gtk.Scale scale;
    [Gtk.Connect] private readonly Gtk.SearchEntry searchEntry;
    [Gtk.Connect] private readonly Gtk.FlowBox flowBox;
    [Gtk.Connect] private readonly Adw.Spinner spinner;
    private string title = "";
    private string subtitle = "";
    private ConfigurationManager configuration;
    private string imageUrl = "https://community.cloudflare.steamstatic.com/economy/image/";
    private string baseURLstart = "https://steamcommunity.com/inventory/";
    private string baseURLend = "/730/2";
    private string baseURL;
    private int minSize = 240;
    private int maxSize = 680;
    private int defaultSize = 380;
    MainApp mainApp;
    private Gtk.CssProvider cssProvider = new Gtk.CssProvider();
    private Gdk.Display display = Gdk.Display.GetDefault();

    List<ItemBox> itemBoxList = new List<ItemBox>();
    private InventoryWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public InventoryWindow(MainApp mainApp, ConfigurationManager configuration) : this(new Gtk.Builder("InventoryWindow.ui"), "inventory_window")
    {
        this.mainApp = mainApp;
        this.configuration = configuration;
        baseURL = baseURLstart + configuration.SteamProfile + baseURLend;
        OnMap += (_, _) => mainApp.SetTitle(title, subtitle);
        SetTitle("Inventory");
        OnRealize += (sender, e) => Fetch();
        //scale
        scale.SetRange(minSize, maxSize);
        scale.SetValue(defaultSize);
        //mark the scale
        scale.AddMark(defaultSize, Gtk.PositionType.Top, "");
        scale.AddMark(0, Gtk.PositionType.Top, "");
        scale.AddMark(maxSize, Gtk.PositionType.Top, "");
        scale.OnValueChanged += (_, _) =>
        {
            SetImageSizeValue((int)scale.GetValue());

        };
        SetImageSizeValue(defaultSize);
        //search entry
        searchEntry.OnSearchChanged += (_, _) => SearchFilter(searchEntry.GetText());
    }


    private void SetImageSizeValue(int value)
    {
        string cssData = ".imageItemBox { min-height: " + value.ToString() + "px ; min-width: " + value.ToString() + "px ;}";
        cssProvider.LoadFromString(cssData);
        Gtk.StyleContext.AddProviderForDisplay(display, cssProvider, 0);

    }
    private void SetTitle(string title, string subtitle = "")
    {
        this.title = title;
        this.subtitle = subtitle;
        mainApp.SetTitle(title, subtitle);
    }
    private void CleanChildren()
    {
        spinner.SetVisible(false);
        Gtk.Widget toRemove = flowBox.GetLastChild();
        while (toRemove != null)
        {
            flowBox.Remove(toRemove);
            toRemove = flowBox.GetLastChild();
        }
    }
    private void HideChildren()
    {
        foreach (ItemBox item in itemBoxList)
        {
            item.GetParent().SetVisible(false);

        }
    }
    private void SetLoadingScreen()
    {
        spinner.SetVisible(true);
    }
    private void Fetch()
    {
        CleanChildren();
        SetLoadingScreen();
        FetchData();
    }
    private void SearchFilter(string search)
    {
        HideChildren();
        foreach (ItemBox item in itemBoxList)
        {
            if (item.name.Contains(search, StringComparison.InvariantCultureIgnoreCase))
            {
                item.GetParent().SetVisible(true);
            }
        }
    }
    private void SetData(string data)
    {
        CleanChildren();
        JObject obj = JObject.Parse(data);
        JToken items = obj.SelectToken("$.descriptions");
        int limit = configuration.ItemsNumber;
        foreach (JToken item in items)
        {
            //make sure & is handled correctly
            var namee = item.SelectToken("$.market_name").ToString();
            if (namee != null)
            {
                var type = item.SelectToken("$.type");
                var imageUrlName = item.SelectToken("$.icon_url");
                var color = item.SelectToken("$.name_color");
                var marketHashName = item.SelectToken("$.market_hash_name");
                //get rarity color and item type
                var tags = item.SelectToken("$.tags");
                var itemTag = type;
                string inspectUrlString = "";
                var inspectUrl = item.SelectToken("$.market_actions");
                if (inspectUrl != null && inspectUrl[0].SelectToken("$.link") != null)
                {
                    inspectUrlString = inspectUrl[0].SelectToken("$.link").ToString();
                }
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
                ItemBox itemBox = new ItemBox(namee.ToString(), type.ToString(), imageUrl + imageUrlName.ToString() + "/?allow_animated=0", color.ToString(), itemTag.ToString(), marketHashName.ToString(), inspectUrlString, configuration.AutoFetchPrices, configuration.Currency);
                itemBoxList.Add(itemBox);
                flowBox.Append(itemBox);
                limit--;
                if (limit == 0)
                {
                    break;
                }
            }
        }

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
