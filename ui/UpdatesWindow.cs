namespace ui;

using Newtonsoft.Json.Linq;

public class UpdatesWindow : Gtk.Box
{
    [Gtk.Connect] private readonly Adw.Carousel carousel;
    [Gtk.Connect] private readonly Gtk.Button button_left;
    [Gtk.Connect] private readonly Gtk.Button button_right;
    private uint currentPos = 0;
    private ConfigurationManager configuration;
    private MainWindow mainWindow;
    private UpdatesWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public UpdatesWindow(MainWindow mainWindow, ConfigurationManager configuration) : this(new Gtk.Builder("UpdatesWindow.ui"), "updates_window")
    {
        this.mainWindow = mainWindow;
        this.configuration = configuration;
        button_right.OnClicked += (_, _) => MoveToNextPage();
        button_left.OnClicked += (_, _) => MoveToPrevPage();
        OnRealize += (sender, e) => Fetch();
        carousel.OnPageChanged += (_, e) => { currentPos = e.Index; };
        OnMap += (_, _) => mainWindow.SetTitle("Game Updates");
        mainWindow.SetTitle("Game Updates");
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
            carousel.Append(Post(contents, dateTime.ToString("dddd, dd MMMM yyyy H:mm:ss"), title, feedname, url));
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
    private string FormatTextForMarkup(string textToFormat)
    {
        textToFormat = textToFormat.Replace("[h3]", "<span font_weight=\"bold\" size=\"x-large\">").Replace("[/h3]", "</span>");
        textToFormat = textToFormat.Replace("[b]", "<b>").Replace("[/b]", "</b>");
        textToFormat = textToFormat.Replace("[i]", "<i>").Replace("[/i]", "</i>");
        textToFormat = textToFormat.Replace("[*]", " • ");
        textToFormat = textToFormat.Replace("[h2]", "<span font_weight=\"bold\" size=\"large\">").Replace("[/h2]", "</span>");
        textToFormat = System.Text.RegularExpressions.Regex.Replace(textToFormat, @"\[(.*?)\]", "");
        return textToFormat;
    }
    private Gtk.ScrolledWindow Post(string contents, string date, string title, string feedname, string url)
    {
        Gtk.ScrolledWindow scrolledWindow = new Gtk.ScrolledWindow();
        Gtk.Box box = new Gtk.Box();
        box.SetOrientation(Gtk.Orientation.Vertical);
        //title
        Gtk.Label titleLabel = new Gtk.Label();
        Gtk.Label titleLabel2 = new Gtk.Label();
        box.Append(titleLabel);
        box.Append(titleLabel2);
        titleLabel.SetText(title);
        titleLabel.AddCssClass("title-1");
        titleLabel.SetHexpand(true);
        titleLabel.SetMarginBottom(5);
        titleLabel2.SetText(feedname);
        titleLabel2.AddCssClass("title-2");
        titleLabel2.SetHexpand(true);
        titleLabel2.SetMarginBottom(10);
        Gtk.Box boxleft = new Gtk.Box();
        boxleft.SetVexpand(true);
        boxleft.SetHexpand(true);
        boxleft.SetOrientation(Gtk.Orientation.Vertical);
        Gtk.Label contentsLabel = new Gtk.Label();
        contentsLabel.Wrap = true;
        contentsLabel.SetUseMarkup(true);
        if (configuration.UseMarkup)
        {
            contents = FormatTextForMarkup(contents);
        }
        contentsLabel.SetText(contents);
        if (configuration.UseMarkup)
        {
            contentsLabel.SetMarkup(contents);
        }
        contentsLabel.SetHexpand(true);
        contentsLabel.SetVexpand(true);

        Gtk.Box boxInfo = new Gtk.Box();
        boxInfo.SetMarginBottom(10);
        //boxleft.SetVexpand(true);
        //boxleft.SetHexpand(true);
        Gtk.Label dateLabel = new Gtk.Label();

        dateLabel.SetText(date);
        dateLabel.AddCssClass("title-4");
        Gtk.Label urlLabel = new Gtk.Label();
        urlLabel.SetXalign(1);
        urlLabel.SetValign(Gtk.Align.Center);
        urlLabel.SetHexpand(true);
         
        urlLabel.SetMarkup("<a href='" + url +"'>Web link</a>");
        
        urlLabel.SetTooltipText(url);
        
        boxInfo.Append(dateLabel);
        boxInfo.Append(urlLabel);
        boxleft.Append(boxInfo);
        boxleft.Append(contentsLabel);
        box.Append(boxleft);
        scrolledWindow.SetChild(box);
        scrolledWindow.SetHexpand(true);
        scrolledWindow.SetVexpand(true);
        box.SetMarginEnd(30);
        box.SetMarginStart(30);
        box.SetMarginBottom(20);
        box.SetHexpand(true);
        box.SetVexpand(true);
        return scrolledWindow;
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
