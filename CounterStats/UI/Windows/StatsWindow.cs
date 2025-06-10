namespace CounterStats.UI.Windows;

using Newtonsoft.Json.Linq;

public class StatsWindow : Gtk.Box, IWindow
{
    [Gtk.Connect] private readonly Gtk.Box _statsWindowBox;
    [Gtk.Connect] private readonly Adw.Banner _banner;
    public string WindowName { get; }
    public string IconName { get; }
    private MainApp _mainApp;
    private ConfigurationManager _configuration;
    string baseURL = $"https://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v2/?appid=730&key=";

    private StatsWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }

    public StatsWindow(MainApp mainApp, ConfigurationManager configuration, string windowName, string iconName) : this(new Gtk.Builder("StatsWindow.ui"), "_root")
    {
        WindowName = windowName;
        _configuration = configuration;
        _mainApp = mainApp;
        IconName = iconName;
        OnRealize += (sender, e) => Refresh();
        OnMap += (_, _) => mainApp.SetTitle("Player Statstics");
        mainApp.SetTitle("Player Statstics");
    }

    public void CleanChildren()
    {
        Gtk.Widget toRemove = _statsWindowBox.GetLastChild();
        //clear window
        while (toRemove != null)
        {
            _statsWindowBox.Remove(toRemove);
            toRemove = _statsWindowBox.GetLastChild();
        }
    }

    public void Refresh()
    {
        CleanChildren();
        Adw.Spinner spinner = new Adw.Spinner();
        spinner.SetHexpand(true);
        spinner.SetVexpand(true);
        _statsWindowBox.Append(spinner);
        SetDataAsync();
    }

    private async Task SetDataAsync()
    {
        string url = baseURL + _configuration.ApiKey + "&steamid=" + _configuration.SteamProfile;
        string data = await Globals.FetchData(url);
        SetData(data);
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
}
