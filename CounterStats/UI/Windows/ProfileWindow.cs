namespace CounterStats.UI.Windows;

using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

public class ProfileWindow : Gtk.Box, IWindow
{
    [Gtk.Connect] private readonly Gtk.Image _profileImage;
    [Gtk.Connect] private readonly Gtk.Label _labelRealName;
    [Gtk.Connect] private readonly Gtk.Label _labelLocation;
    [Gtk.Connect] private readonly Gtk.Label _labelName;
    [Gtk.Connect] private readonly Gtk.Label _labelTimeCreated;
    [Gtk.Connect] private readonly Gtk.Label _labelSummary;
    [Gtk.Connect] private readonly Gtk.Label _ratioLabel;
    [Gtk.Connect] private readonly Gtk.Label _ratioLabelValue;
    [Gtk.Connect] private readonly Gtk.Label _hsLabel;
    [Gtk.Connect] private readonly Gtk.Label _hsLabelValue;
    [Gtk.Connect] private readonly Gtk.Label _timeLabel;
    [Gtk.Connect] private readonly Gtk.Label _timeLabelValue;
    [Gtk.Connect] private readonly Gtk.Label _labelVac;
    [Gtk.Connect] private readonly Adw.Banner _banner;
    public string WindowName { get; }
    public string IconName { get; }
    private string title = "";
    private string subtitle = "";
    private ConfigurationManager _configuration;
    private MainApp _mainWindow;
    private string steamProfileID;

    private ProfileWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }

    public ProfileWindow(MainApp mainWindow, ConfigurationManager configuration, string windowName, string iconName, string steamProfile = null) : this(new Gtk.Builder("ProfileWindow.ui"), "_root")
    {
        if (string.IsNullOrEmpty(steamProfile))
        {
            this.steamProfileID = configuration.SteamProfile;
        }
        else
        {
            this.steamProfileID = steamProfile;
        }
        _mainWindow = mainWindow;
        _configuration = configuration;
        WindowName = windowName;
        IconName = iconName;
        OnRealize += (sender, e) => Refresh();
        OnMap += (_, _) => mainWindow.SetTitle(title, subtitle);
        if (string.IsNullOrEmpty(steamProfile))
        {
            SetTitle("Your Profile");
        }
        else
        {
            SetTitle("Profile");
        }
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
        if (String.IsNullOrEmpty(steamProfileID))
        {
            SetBanner("Steam Profile ID is empty, make sure to set it in the prefrences");
            return;
        }
        SetProfileDataAsync();
        SetBackgroundAsync();
        if (String.IsNullOrEmpty(_configuration.ApiKey))
        {
            SetBanner("API is empty, make sure to set it in the prefrences");
            return;
        }
        GetStatsAsync();
    }
    private async Task SetProfileDataAsync()
    {
        //string url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + _configuration.ApiKey + "&steamids=" + steamProfile;
        string url = "https://steamcommunity.com/profiles/" + steamProfileID + "/?xml=1";
        string data = await Globals.FetchData(url);
        SteamProfile steamProfile = new SteamProfile(data);
        SetData(steamProfile);
    }
    private async Task GetStatsAsync()
    {
        string url = $"https://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v2/?appid=730&key=" + _configuration.ApiKey + "&steamid=" + steamProfileID;
        string data = await Globals.FetchData(url);
        SetStats(data);
    }

    private async Task SetBackgroundAsync()
    {
        string url = $"https://api.steampowered.com/IPlayerService/GetProfileBackground/v1/?steamid=" + steamProfileID;

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

    private void SetData(SteamProfile steamProfile)
    {

        _labelName.SetText(steamProfile.Name);
        SetTitle("Your Profile", steamProfile.Name);

        if (!string.IsNullOrEmpty(steamProfile.Location))
        {
            _labelLocation.SetText(steamProfile.Location);
        }
        if (!string.IsNullOrEmpty(steamProfile.Avatar))
        {
            SetAvatar(steamProfile.Avatar);
        }
        if (!string.IsNullOrEmpty(steamProfile.SecondaryName))
        {
            _labelRealName.SetText(steamProfile.SecondaryName);
        }
        if (!string.IsNullOrEmpty(steamProfile.TimeCreated))
        {
            _labelTimeCreated.SetText(steamProfile.TimeCreated);
        }
        if (!string.IsNullOrEmpty(steamProfile.Summary))
        {
            _labelSummary.SetText(Regex.Replace(steamProfile.Summary, "<.*?>", String.Empty));
            _labelSummary.SetMarkup(steamProfile.Summary);
        }
        if (!string.IsNullOrEmpty(steamProfile.VacBanned))
        {
            if (Convert.ToInt32(steamProfile.VacBanned) == 0)
            {
                _labelVac.SetMarkup("<span color=\"" + Globals.COLOR_GREEN + "\">Account In Good VAC Standing</span>");
            }
            else
            {
                _labelVac.SetMarkup("<span color=\"" + Globals.COLOR_RED + "\"><b>VAC BANNED!!!!!!!!!!!!!</b></span>");
            }
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

    private async void SetStats(string data)
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
