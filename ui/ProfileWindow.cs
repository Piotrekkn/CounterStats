namespace ui;
using Newtonsoft.Json.Linq;
using Gtk;

public class ProfileWindow : Gtk.Box
{

    [Gtk.Connect] private readonly Gtk.Box profileWindowBox;
    [Gtk.Connect] private readonly Gtk.Box imageBox;
    [Gtk.Connect] private readonly Gtk.Image profileImage;
    [Gtk.Connect] private readonly Gtk.Box profileWindow;
    [Gtk.Connect] private readonly Gtk.Label labelRealName;
    [Gtk.Connect] private readonly Gtk.Label labelLocation;
    [Gtk.Connect] private readonly Gtk.Label labelName;
    [Gtk.Connect] private readonly Gtk.Label timeCreated;
    [Gtk.Connect] private readonly Gtk.Label ratio_label;
    [Gtk.Connect] private readonly Gtk.Label ratio_label_value;
    [Gtk.Connect] private readonly Gtk.Label hs_label;
    [Gtk.Connect] private readonly Gtk.Label hs_label_value;
    [Gtk.Connect] private readonly Gtk.Label time_label;
    [Gtk.Connect] private readonly Gtk.Label time_label_value;
    [Gtk.Connect] private readonly Adw.Banner banner;
    private string title = "";
    private string subtitle = "";
    private ConfigurationManager configuration;
    private MainWindow mainWindow;

    private ProfileWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public ProfileWindow(MainWindow mainWindow, ConfigurationManager configuration) : this(new Gtk.Builder("ProfileWindow.ui"), "profileWindow")
    {
        this.mainWindow = mainWindow;
        this.configuration = configuration;
        OnRealize += (sender, e) => Fetch();
        OnMap += (_, _) => mainWindow.SetTitle(title, subtitle);
        SetTitle("Your Profile");

    }
    private void SetTitle(string title, string subtitle = "")
    {
        this.title = title;
        this.subtitle = subtitle;
        mainWindow.SetTitle(title, subtitle);
    }


    private void CleanChildren()
    {
        return;
        Gtk.Widget toRemove = profileWindowBox.GetLastChild();
        //clear window
        while (toRemove != null)
        {
            profileWindowBox.Remove(toRemove);
            toRemove = profileWindowBox.GetLastChild();
        }

    }
    private async void SetBackground()
    {
        string baseURL = $"https://api.steampowered.com/IPlayerService/GetProfileBackground/v1/?key=" + configuration.ApiKey + "&steamid=" + configuration.SteamProfile;
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

                            string dataa = ".profileWindowBox { background-image: " + "url(\'file://" + dir + "\');\n background-size: cover;}";
                            Gtk.CssProvider cssProvider = new Gtk.CssProvider();
                            cssProvider.LoadFromString(dataa);
                            profileWindow.AddCssClass("profileWindowBox");
                            Gdk.Display display = Gdk.Display.GetDefault();
                            StyleContext.AddProviderForDisplay(display, cssProvider, 0);

                        }
                        else
                        {
                            Console.WriteLine("Data is null!");
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

    private Gtk.Image imageFromUrl(string url)
    {
        Gtk.Image image = new Gtk.Image();
        image.SetFromFile(url);
        return image;
    }

    private void Loading()
    {
        CleanChildren();
        return;
        Gtk.Box box = new Gtk.Box();
        box.SetHexpand(true);
        box.SetVexpand(true);
        Gtk.Spinner spinner = new Gtk.Spinner();
        Gtk.Label label = new Gtk.Label();
        label.SetHexpand(true);
        label.SetVexpand(true);
        label.SetText("Loading");
        profileWindowBox.Append(box);
        box.Append(spinner);
        box.Append(label);

    }

    private void Fetch()
    {
        banner.SetRevealed(false);
        Loading();
        if (String.IsNullOrEmpty(configuration.ApiKey))
        {
            SetBanner("API is empty, make sure to set it in the prefrences");
            return;
        }
        FetchData();
    }

    private void SetBanner(string text)
    {
        Console.WriteLine($"Banner: {text}");

        banner.SetTitle(text);
        banner.SetRevealed(true);


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
        labelName.SetText(player.SelectToken("$.personaname").ToString());
        SetTitle("Your Profile", player.SelectToken("$.personaname").ToString());
        if (player.SelectToken("$.realname") != null)
            labelRealName.SetText(player.SelectToken("$.realname").ToString());
        if (player.SelectToken("$.loccityid") != null)
            labelLocation.SetText(player.SelectToken("$.loccityid").ToString() + ", " + player.SelectToken("$.locstatecode").ToString() + ", " + player.SelectToken("$.loccountrycode").ToString());
        if (player.SelectToken("$.avatarfull") != null)
        {
            string avatar_url = player.SelectToken("$.avatarfull").ToString();
            SetAvatar(avatar_url);
        }
        if (player.SelectToken("$.timecreated") != null)
        {
            long dateLong = (long)Convert.ToDouble(player.SelectToken("$.timecreated").ToString());
            System.DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(dateLong).LocalDateTime;
            timeCreated.SetText("Account created:\n" + dateTime.ToString("dddd, dd MMMM yyyy H:mm:ss"));
        }
        SetBackground();
        FetchData2();
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
        profileImage.SetFromFile(dir);
    }
    private async void FetchData()
    {// 
        string baseURL = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + configuration.ApiKey + "&steamids=" + configuration.SteamProfile;
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
                            SetData(data);
                        }
                        else
                        {
                            Console.WriteLine("Data is null!");
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

    private async void FetchData2()
    {
        string baseURL = $"https://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v2/?appid=730&key=" + configuration.ApiKey + "&steamid=" + configuration.SteamProfile; ;
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
                            SetData2(data);
                        }
                        else
                        {
                            Console.WriteLine("Data is null!");
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
    private async void SetData2(string data)
    {
        JObject obj = JObject.Parse(data);
        time_label.SetLabel("Time spent in game");
        hs_label.SetLabel("HS percentage");
        ratio_label.SetLabel("KD Ratio");
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

            time_label_value.SetLabel(((int)total_time_played / 60 / 60).ToString() + "h");
            hs_label_value.SetLabel(((double)total_kills_headshot * 100 / (double)total_kills).ToString("0.###") + "%");
            ratio_label_value.SetLabel(((double)total_kills / (double)total_deaths).ToString("0.###"));
        }
    }
}
