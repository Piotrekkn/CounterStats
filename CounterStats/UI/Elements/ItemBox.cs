namespace CounterStats.UI.Elements;

using System.Net;
using System.Threading.Tasks;
using CounterStats.Enums;
using GdkPixbuf;
using GLib;
using Newtonsoft.Json.Linq;
public class ItemBox : Gtk.Box
{
    [Gtk.Connect] private readonly Gtk.Image image;
    [Gtk.Connect] private readonly Gtk.Label labelInspect;
    [Gtk.Connect] private readonly Gtk.Label labelName;
    [Gtk.Connect] private readonly Gtk.Label labelTag;
    [Gtk.Connect] private readonly Gtk.Label labelType;
    [Gtk.Connect] private readonly Gtk.Label labelPrice;
    [Gtk.Connect] private readonly Gtk.Button buttonPrice;
    private string marketPriceUrl = "https://steamcommunity.com/market/priceoverview/?appid=730&market_hash_name=";
    private string marketCurrencyUrl = "&currency=";
    private int currency;

    public string name="";
    private ItemBox(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public ItemBox(string name, string type, string imageUrl, string color, string itemTag, string marketHashName, string inspectUrl, bool autoFetchPrices, int currency) : this(new Gtk.Builder("ItemBox.ui"), "itemBox")
    {
        this.currency = currency;
        marketCurrencyUrl += (currency + 1).ToString();
        this.name = name;
        labelName.SetLabel(name);
        labelTag.SetLabel(itemTag);
        labelType.SetLabel(type);

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
        //set inspect
        if (!string.IsNullOrEmpty(inspectUrl))
        {
            labelInspect.SetMarkup("<a href='" + inspectUrl.ToString() + "'>Inspect</a>");
        }
        buttonPrice.OnClicked += (_, _) => { SetItemPrice(marketHashName); };
        //set aditional data
        SetItemImage(image, imageUrl);
        //get prices
        if (autoFetchPrices)
        {
            SetItemPrice(marketHashName);
        }

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
    private async Task SetItemPrice(string marketHashName)
    {
        buttonPrice.SetVisible(false);
        //handle & correctly
        string url = marketPriceUrl + marketHashName.Replace("&", "%26") + marketCurrencyUrl;
        try
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(url))
                {
                    res.EnsureSuccessStatusCode();
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        if (data != null)
                        {
                            JObject obj = JObject.Parse(data);
                            Console.WriteLine(data.ToString());
                            JToken price = obj.SelectToken("$.lowest_price");
                            //sometimes only median price is avaliable
                            if (price == null)
                            {
                                price = obj.SelectToken("$.median_price");
                            }
                            //some items doesnt have price at all
                            if (price == null)
                            {
                                labelPrice.SetText("Priceless");
                            }
                            else
                            {
                                labelPrice.SetText(price.ToString());
                            }
                            labelPrice.SetCssClasses(["title-4"]);
                        }
                        else
                        {
                            buttonPrice.SetVisible(true);
                            //alert
                        }
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            labelPrice.SetText("Too many requests");
            buttonPrice.SetVisible(true);
            labelPrice.SetCssClasses([""]);
        }
    }
    private static Pixbuf FromBytes(byte[] data)
    {
        using var bytes = Bytes.New(data);
        var loader = PixbufLoader.New();
        loader.WriteBytes(bytes);
        loader.Close();
        return loader.GetPixbuf() ?? throw new Exception("No Pixbuf created.");
    }
}
