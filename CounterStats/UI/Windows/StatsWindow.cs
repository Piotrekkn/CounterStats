namespace CounterStats.UI.Windows;

using Newtonsoft.Json.Linq;

public class StatsWindow : Gtk.Box
{

    [Gtk.Connect] private readonly Gtk.Box _statsWindowBox;
    [Gtk.Connect] private readonly Adw.Banner _banner;
    private MainApp _mainApp;
    private ConfigurationManager _configuration;
    private StatsWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public StatsWindow(MainApp mainApp, ConfigurationManager configuration) : this(new Gtk.Builder("StatsWindow.ui"), "_root")
    {
        _configuration = configuration;
        _mainApp = mainApp;
        OnRealize += (sender, e) => Fetch();
        OnMap += (_, _) => mainApp.SetTitle("Player Statstics");
        mainApp.SetTitle("Player Statstics");
    }
    private void CleanChildren()
    {
        Gtk.Widget toRemove = _statsWindowBox.GetLastChild();
        //clear window
        while (toRemove != null)
        {
            _statsWindowBox.Remove(toRemove);
            toRemove = _statsWindowBox.GetLastChild();
        }
    }
    private void Fetch()
    {
        FetchData();
    }
    private void SetData(string data)
    {
        CleanChildren();
        if (data.Contains("Please verify your <pre>key=</pre> parameter"))
        {
            SetBanner("API is wrong, make sure correct api is set in prefrences");
            return;
        }
        if (data.Contains("Unknown problem determining WebApi request"))
        {
            SetBanner("Profile id is wrong, make sure correct id is set in prefrences");
            return;
        }
        Gtk.Box box = new Gtk.Box();
        box.SetHexpand(true);
        box.SetVexpand(true);
        Adw.PreferencesPage preferencesPage = new Adw.PreferencesPage();
        preferencesPage.SetHexpand(true);
        box.Append(preferencesPage);
        _statsWindowBox.Append(box);
        JObject obj = JObject.Parse(data);
        JToken stats = obj.SelectToken("$.playerstats").SelectToken("$.stats");
        foreach (JToken token in stats)
        {
            string name = token.SelectToken("$.name").ToString();
            //capitalize first letter and change to spaces
            name = (name[0].ToString().ToUpper() + name.Substring(1, name.Length - 1)).Replace("_", " ");
            preferencesPage.Add(preferencesGroup(name, token.SelectToken("$.value").ToString()));
        }
    }
    private void SetBanner(string text)
    {
        Console.WriteLine($"Banner: {text}");
        _banner.SetTitle(text);
        _banner.SetRevealed(true);
    }
    private Adw.PreferencesGroup preferencesGroup(string title, string subtitle)
    {
        Adw.ActionRow actionRow = new Adw.ActionRow();
        Adw.PreferencesGroup preferencesGroup = new Adw.PreferencesGroup();
        preferencesGroup.Add(actionRow);
        preferencesGroup.SetHexpand(true);
        preferencesGroup.SetVexpand(true);
        actionRow.Title = title;
        actionRow.Subtitle = subtitle;
        return preferencesGroup;
    }

    private async void FetchData()
    {
        string baseURL = $"https://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v2/?appid=730&key=" + _configuration.ApiKey + "&steamid=" + _configuration.SteamProfile;
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
