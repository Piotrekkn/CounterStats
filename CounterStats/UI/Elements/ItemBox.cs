namespace CounterStats.UI.Elements;

using System.Net;
using System.Threading.Tasks;
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
    private string marketPriceUrl = "https://steamcommunity.com/market/priceoverview/?appid=730&currency=1&market_hash_name=";
    private ItemBox(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public ItemBox(string name, string type, string imageUrl, string color, string itemTag, string marketHashName, string inspectUrl) : this(new Gtk.Builder("ItemBox.ui"), "itemBox")
    {
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
        //set aditional data
        SetItemImage(image, imageUrl);

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
    private async Task SetItemPrice(Gtk.Label labelPrice, string marketHashName)
    {
        string url = marketPriceUrl + marketHashName;
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
                            JToken price = obj.SelectToken("$.lowest_price");
                            labelPrice.SetText(price.ToString());
                        }
                        else
                        {
                            //alert
                        }
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            labelPrice.SetText("too many request");
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
