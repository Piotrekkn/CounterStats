namespace CounterStats.UI.Windows;

using CounterStats.UI.Elements;
using Newtonsoft.Json.Linq;

public class UpdatesWindow : Gtk.Box
{
    [Gtk.Connect] private readonly Adw.Carousel carousel;
    [Gtk.Connect] private readonly Gtk.Button button_left;
    [Gtk.Connect] private readonly Gtk.Button button_right;
    private uint currentPos = 0;
    private ConfigurationManager configuration;
    private MainApp mainApp;
    private UpdatesWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public UpdatesWindow(MainApp mainApp, ConfigurationManager configuration) : this(new Gtk.Builder("UpdatesWindow.ui"), "updates_window")
    {
        this.mainApp = mainApp;
        this.configuration = configuration;
        button_right.OnClicked += (_, _) => MoveToNextPage();
        button_left.OnClicked += (_, _) => MoveToPrevPage();
        OnRealize += (sender, e) => Fetch();
        carousel.OnPageChanged += (_, e) => { currentPos = e.Index; };
        OnMap += (_, _) => mainApp.SetTitle("Game Updates");
        mainApp.SetTitle("Game Updates");
    }

    private void AppendPosts(string data)
    {
        if (data == null)
        {
            return;
        }
        JObject obj = JObject.Parse(data);
        JToken appnews = obj.SelectToken("$.appnews");

        foreach (JToken item in appnews.SelectToken("$.newsitems"))
        {
            string contents = item.SelectToken("$.contents").ToString();
            string title = item.SelectToken("$.title").ToString();
            string url = item.SelectToken("$.url").ToString();
            string feedname = item.SelectToken("$.feedlabel").ToString();
            //date
            string date = item.SelectToken("$.date").ToString();
            long dateLong = (long)Convert.ToDouble(date);
            System.DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(dateLong).LocalDateTime;
            //append
            carousel.Append(new UpdatePost(contents, dateTime.ToString("dddd, dd MMMM yyyy H:mm:ss"), title, feedname, url,configuration.UseMarkup));
        }
    }
    private void MoveToNextPage()
    {
        if (currentPos < carousel.GetNPages() - 1)
        {
            carousel.ScrollTo(carousel.GetNthPage(currentPos + 1), true);
        }
    }
    private void MoveToPrevPage()
    {
        if (currentPos > 0)
        {
            carousel.ScrollTo(carousel.GetNthPage(currentPos - 1), true);
        }
    }
  
    private void CleanChildren()
    {
        Gtk.Widget toRemove = carousel.GetLastChild();
        //clear window
        while (toRemove != null)
        {
            carousel.Remove(toRemove);
            toRemove = carousel.GetLastChild();
        }

    }
    private void Fetch()
    {
        FetchData();
    }

    private async void FetchData()
    {
        string baseURL = $"https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=730&count=" + configuration.UpdatesNumber;
        try
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(baseURL))
                {
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        if (data != null)
                        {
                            CleanChildren();
                            AppendPosts(data);
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
