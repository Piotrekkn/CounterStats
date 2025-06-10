namespace CounterStats.UI.Windows;

using Newtonsoft.Json.Linq;

public class ProfileWindow : Gtk.Box, IWindow
{
    [Gtk.Connect] private readonly Gtk.Image _profileImage;
    [Gtk.Connect] private readonly Gtk.Label _labelRealName;
    [Gtk.Connect] private readonly Gtk.Label _labelLocation;
    [Gtk.Connect] private readonly Gtk.Label _labelName;
    [Gtk.Connect] private readonly Gtk.Label _timeCreated;
    [Gtk.Connect] private readonly Gtk.Label _ratioLabel;
    [Gtk.Connect] private readonly Gtk.Label _ratioLabelValue;
    [Gtk.Connect] private readonly Gtk.Label _hsLabel;
    [Gtk.Connect] private readonly Gtk.Label _hsLabelValue;
    [Gtk.Connect] private readonly Gtk.Label _timeLabel;
    [Gtk.Connect] private readonly Gtk.Label _timeLabelValue;
    [Gtk.Connect] private readonly Adw.Banner _banner;
    public string WindowName { get; }
    public string IconName { get; }
    private string title = "";
    private string subtitle = "";
    private ConfigurationManager _configuration;
    private MainApp _mainWindow;

    private ProfileWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }

    public ProfileWindow(MainApp mainWindow, ConfigurationManager configuration, string windowName, string iconName) : this(new Gtk.Builder("ProfileWindow.ui"), "_root")
    {
        _mainWindow = mainWindow;
        _configuration = configuration;
        WindowName = windowName;
        IconName = iconName;
        OnRealize += (sender, e) => Refresh();
        OnMap += (_, _) => mainWindow.SetTitle(title, subtitle);
        SetTitle("Your Profile");
    }

    private void SetTitle(string title, string subtitle = "")
    {
        this.title = title;
        this.subtitle = subtitle;
        _mainWindow.SetTitle(title, subtitle);
    }

    public void Refresh()
    {
        _banner.SetRevealed(false);

        if (String.IsNullOrEmpty(_configuration.ApiKey))
        {
            SetBanner("API is empty, make sure to set it in the prefrences");
            return;
        }
        //TODO
        SetDataAsync();
        SetDataAsync2();
        SetBackgroundAsync();
    }
    private async Task SetDataAsync()
    {
        string url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + _configuration.ApiKey + "&steamids=" + _configuration.SteamProfile;
        string data = await Globals.FetchData(url);
        SetData(data);
    }
    private async Task SetDataAsync2()
    {
        string url = $"https://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v2/?appid=730&key=" + _configuration.ApiKey + "&steamid=" + _configuration.SteamProfile;
        string data = await Globals.FetchData(url);
        SetData2(data);
    }

    private async Task SetBackgroundAsync()
    {
        string url = $"https://api.steampowered.com/IPlayerService/GetProfileBackground/v1/?key=" + _configuration.ApiKey + "&steamid=" + _configuration.SteamProfile;
        string data = await Globals.FetchData(url);

        if (data != null)
        {
            JObject obj = JObject.Parse(data);
            if (obj.SelectToken("$.response").SelectToken("$.profile_background") == null)
            {
                return;
            }
            JToken player = obj.SelectToken("$.response").SelectToken("$.profile_background").SelectToken("$.image_large");
            string image = "https://cdn.fastly.steamstatic.com/steamcommunity/public/images/" + player.ToString();

            string dir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.cache/counterstats/background.jpg";
            if (!System.IO.File.Exists(dir))
            {
                using var client2 = new HttpClient();
                byte[] imageBytes = await client2.GetByteArrayAsync(image);
                await System.IO.File.WriteAllBytesAsync(dir, imageBytes);
            }
            string cssData = ".profileWindowBox { background-image: " + "url(\'file://" + dir + "\');\n background-size: cover;}";
            Gtk.CssProvider cssProvider = new Gtk.CssProvider();
            cssProvider.LoadFromString(cssData);
            AddCssClass("profileWindowBox");
            Gdk.Display display = Gdk.Display.GetDefault();
            Gtk.StyleContext.AddProviderForDisplay(display, cssProvider, 0);
        }
        else
        {
            Console.WriteLine("Data is null!");
        }
    }

    public void CleanChildren()
    {

    }

    private void SetBanner(string text)
    {
        Console.WriteLine($"Banner: {text}");
        _banner.SetTitle(text);
        _banner.SetRevealed(true);
    }

    private async void SetData(string data)
    {
        JObject obj;
        if (data.Contains("{\"response\":{\"players\":[]}}"))
        {
            SetBanner("Profile id is wrong, make sure correct id is set in prefrences");
            return;
        }
        try
        {
            obj = JObject.Parse(data);
        }
        catch (System.Exception)
        {
            if (data.Contains("Please verify your <pre>key=</pre> parameter."))
            {
                SetBanner("API is wrong, make sure correct api is set in prefrences");
            }
            else if (data.Contains("429 Too Many Requests"))
            {
                SetBanner("Too many request");
            }
            else
            {
                SetBanner("Error parsing data");
            }
            return;
        }
        //check for profile id
        JToken player = obj.SelectToken("$.response").SelectToken("$.players")[0];
        Console.WriteLine(player.SelectToken("$.avatar").ToString());
        _labelName.SetText(player.SelectToken("$.personaname").ToString());
        SetTitle("Your Profile", player.SelectToken("$.personaname").ToString());
        if (player.SelectToken("$.realname") != null)
            _labelRealName.SetText(player.SelectToken("$.realname").ToString());
        if (player.SelectToken("$.loccityid") != null)
            _labelLocation.SetText(player.SelectToken("$.loccityid").ToString() + ", " + player.SelectToken("$.locstatecode").ToString() + ", " + player.SelectToken("$.loccountrycode").ToString());
        if (player.SelectToken("$.avatarfull") != null)
        {
            string avatar_url = player.SelectToken("$.avatarfull").ToString();
            SetAvatar(avatar_url);
        }
        if (player.SelectToken("$.timecreated") != null)
        {
            long dateLong = (long)Convert.ToDouble(player.SelectToken("$.timecreated").ToString());
            System.DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(dateLong).LocalDateTime;
            _timeCreated.SetText("Account created:\n" + dateTime.ToString("dddd, dd MMMM yyyy H:mm:ss"));
        }
    }

    private async void SetAvatar(string url)
    {
        string dir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.cache/counterstats/avatar.jpg";
        if (!System.IO.File.Exists(dir))
        {
            using var client = new HttpClient();
            byte[] imageBytes = await client.GetByteArrayAsync(url);
            await System.IO.File.WriteAllBytesAsync(dir, imageBytes);
        }
        _profileImage.SetFromFile(dir);
    }

    private async void SetData2(string data)
    {
        JObject obj = null;
        try
        {
            obj = JObject.Parse(data);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
        if (obj == null)
        {
            return;
        }
        _timeLabel.SetLabel("Time spent in game");
        _hsLabel.SetLabel("HS percentage");
        _ratioLabel.SetLabel("KD Ratio");
        if (obj.SelectToken("$.playerstats") != null)
        {
            JToken stats = obj.SelectToken("$.playerstats").SelectToken("$.stats");
            int total_kills = 0;
            int total_deaths = 0;
            int total_time_played = 0;
            int total_kills_headshot = 0;
            foreach (JToken token in stats)
            {
                //total_kills total_deaths total_time_played total_kills_headshot
                if (token.SelectToken("$.name").ToString() == "total_kills")
                {
                    total_kills = Convert.ToInt32(token.SelectToken("$.value"));
                }
                else if (token.SelectToken("$.name").ToString() == "total_deaths")
                {
                    total_deaths = Convert.ToInt32(token.SelectToken("$.value"));
                }
                else if (token.SelectToken("$.name").ToString() == "total_time_played")
                {
                    total_time_played = Convert.ToInt32(token.SelectToken("$.value"));
                }
                else if (token.SelectToken("$.name").ToString() == "total_kills_headshot")
                {
                    total_kills_headshot = Convert.ToInt32(token.SelectToken("$.value"));
                }
            }
            _timeLabelValue.SetLabel(((int)total_time_played / 60 / 60).ToString() + "h");
            _hsLabelValue.SetLabel(((double)total_kills_headshot * 100 / (double)total_kills).ToString("0.###") + "%");
            _ratioLabelValue.SetLabel(((double)total_kills / (double)total_deaths).ToString("0.###"));
        }
    }
}
