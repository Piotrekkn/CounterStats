namespace CounterStats.UI.Windows;

using CounterStats.UI.Elements;
using Newtonsoft.Json.Linq;

public class InventoryWindow : Gtk.Box
{
    [Gtk.Connect] private readonly Gtk.Box inventory_box;
    private string title = "";
    private string subtitle = "";
    private ConfigurationManager configuration;
    private string imageUrl = "https://community.cloudflare.steamstatic.com/economy/image/";
    private string baseURLstart = "https://steamcommunity.com/inventory/";
    private string baseURLend = "/730/2";
    private string baseURL;
    MainApp mainApp;
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
    }
    private void SetTitle(string title, string subtitle = "")
    {
        this.title = title;
        this.subtitle = subtitle;
        mainApp.SetTitle(title, subtitle);
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
                ItemBox itemBox = new ItemBox(namee.ToString(), type.ToString(), imageUrl + imageUrlName.ToString() + "/?allow_animated=0", color.ToString(), itemTag.ToString(), marketHashName.ToString(), inspectUrlString);
                flowBox.Append(itemBox);
                limit--;
                if (limit == 0)
                {
                    break;
                }
            }
        }
        inventory_box.Append(flowBox);
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
