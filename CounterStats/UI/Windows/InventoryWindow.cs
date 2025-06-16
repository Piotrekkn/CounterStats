namespace CounterStats.UI.Windows;

using CounterStats.UI.Elements;
using Newtonsoft.Json.Linq;

public class InventoryWindow : Gtk.Box, IWindow
{
    [Gtk.Connect] private readonly Gtk.Scale _scale;
    [Gtk.Connect] private readonly Gtk.SearchEntry _searchEntry;
    [Gtk.Connect] private readonly Gtk.FlowBox _flowBox;
    [Gtk.Connect] private readonly Adw.Spinner _spinner;
    public string WindowName { get; }
    public string IconName { get; }
    private string title = "";
    private string subtitle = "";
    private ConfigurationManager _configuration;
    private string imageUrl = "https://community.cloudflare.steamstatic.com/economy/image/";
    private string baseURLstart = "https://steamcommunity.com/inventory/";
    private string baseURLend = "/730/2";
    private int minSize = 240;
    private int maxSize = 680;
    private int defaultSize = 380;
    MainApp _mainApp;
    private Gtk.CssProvider cssProvider = new Gtk.CssProvider();
    private Gdk.Display display = Gdk.Display.GetDefault();

    List<ItemBox> itemBoxList = new List<ItemBox>();

    private InventoryWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }

    public InventoryWindow(MainApp mainApp, ConfigurationManager configuration, string windowName, string iconName) : this(new Gtk.Builder("InventoryWindow.ui"), "_root")
    {
        WindowName = windowName;
        _mainApp = mainApp;
        _configuration = configuration;
        IconName = iconName;
        OnMap += (_, _) => mainApp.SetTitle(title, subtitle);
        SetTitle("Inventory");
        OnRealize += (sender, e) => Refresh();
        //scale
        _scale.SetRange(minSize, maxSize);
        _scale.SetValue(defaultSize);
        //mark the scale
        _scale.AddMark(defaultSize, Gtk.PositionType.Top, "");
        _scale.AddMark(0, Gtk.PositionType.Top, "");
        _scale.AddMark(maxSize, Gtk.PositionType.Top, "");
        _scale.OnValueChanged += (_, _) =>
        {
            SetImageSizeValue((int)_scale.GetValue());
        };
        SetImageSizeValue(defaultSize);
        //search entry
        _searchEntry.OnSearchChanged += (_, _) => SearchFilter(_searchEntry.GetText());
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
        _mainApp.SetTitle(title, subtitle);
    }

    public void CleanChildren()
    {
        itemBoxList = new List<ItemBox>();
        _spinner.SetVisible(false);
        Gtk.Widget toRemove = _flowBox.GetLastChild();
        while (toRemove != null)
        {
            _flowBox.Remove(toRemove);
            toRemove = _flowBox.GetLastChild();
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
        _spinner.SetVisible(true);
    }

    public void Refresh()
    {
        CleanChildren();
        SetLoadingScreen();
        SetDataAsync();
    }
    private async Task SetDataAsync()
    {
        string url = baseURLstart + _configuration.SteamProfile + baseURLend;
        string data = await Globals.FetchData(url);
        SetData(data);
    }
    private void SearchFilter(string search)
    {
        HideChildren();
        foreach (ItemBox item in itemBoxList)
        {
            if (item.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase))
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
        int limit = _configuration.ItemsNumber;
        foreach (JToken item in items)
        {
            //make sure & is handled correctly
            var itemName = item.SelectToken("$.market_name").ToString();
            if (itemName != null)
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
                ItemBox itemBox = new ItemBox(
                    itemName.ToString(),
                    type.ToString(),
                    imageUrl + imageUrlName.ToString() + "/?allow_animated=0",
                    color.ToString(),
                    itemTag.ToString(),
                    marketHashName.ToString(),
                    inspectUrlString,
                    _configuration.AutoFetchPrices,
                    _configuration.Currency
                );
                itemBoxList.Add(itemBox);
                _flowBox.Append(itemBox);
                limit--;
                if (limit == 0)
                {
                    break;
                }
            }
        }
        //filter items
        SearchFilter(_searchEntry.GetText());
    }
}
