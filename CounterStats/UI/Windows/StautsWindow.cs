namespace CounterStats.UI.Windows;

using CounterStats.UI.Elements;
using Newtonsoft.Json.Linq;
public class StatusWindow : Gtk.Box, IWindow
{
    [Gtk.Connect] private readonly Gtk.FlowBox _boxStatus;
    [Gtk.Connect] private readonly Gtk.FlowBox _boxStatusAmericas;
    [Gtk.Connect] private readonly Gtk.FlowBox _boxStatusAsia;
    [Gtk.Connect] private readonly Gtk.FlowBox _boxStatusRest;
    [Gtk.Connect] private readonly Gtk.Box _contentBox;
    [Gtk.Connect] private readonly Adw.Spinner _spinner;
    [Gtk.Connect] private readonly Gtk.Label _refreshLabel;
    [Gtk.Connect] private readonly Gtk.Label _communityLabel;
    [Gtk.Connect] private readonly Gtk.Label _sessionsLabel;
    [Gtk.Connect] private readonly Gtk.Label _leaderboardsLabel;
    [Gtk.Connect] private readonly Gtk.Label _matchmakingLabel;
    [Gtk.Connect] private readonly Adw.Banner _banner;
    public string WindowName { get; }
    public string IconName { get; }
    private string url = "https://api.steampowered.com/ICSGOServers_730/GetGameServersStatus/v1?key=";
    private string apiKey = "";
    private StatusWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }

    public StatusWindow(MainApp mainApp, ConfigurationManager configuration, string windowName, string iconName) : this(new Gtk.Builder("StatusWindow.ui"), "_root")
    {
        OnMap += (_, _) => mainApp.SetTitle(windowName);
        OnRealize += (sender, e) => Refresh();
        WindowName = windowName;
        IconName = iconName;
        apiKey = configuration.ApiKey;
    }

    public void CleanChildren()
    {
        _spinner.SetVisible(false);
        _contentBox.SetVisible(false);
        _communityLabel.SetText("");
        _sessionsLabel.SetText("");
        _leaderboardsLabel.SetText("");
        List<Gtk.FlowBox> widgetsToClear = new List<Gtk.FlowBox>() { _boxStatus, _boxStatusAsia, _boxStatusAmericas, _boxStatusRest };
        foreach (Gtk.FlowBox widget in widgetsToClear)
        {
            Gtk.Widget toRemove = widget.GetLastChild();
            while (toRemove != null)
            {
                widget.Remove(toRemove);
                toRemove = widget.GetLastChild();
            }
        }
    }

    public void Refresh()
    {
        CleanChildren();
        _contentBox.SetVisible(false);
        _spinner.SetVisible(true);
        _banner.SetRevealed(false);
        SetDataAsync();
    }

    private async Task SetDataAsync()
    {
        string fetchURL = url + apiKey;
        string data = await Globals.FetchData(fetchURL);
        SetData(data);
    }
    private void SetBanner(string text)
    {
        Console.WriteLine($"Banner: {text}");
        _banner.SetTitle(text);
        _banner.SetRevealed(true);
    }

    private void SetData(string data)
    {
        if (data.Contains("Access is denied. Retrying will not help. Please verify your"))
        {
            SetBanner("API key is set incorrectly");
            return;
        }
        JObject obj = JObject.Parse(data);
        //refresh time and game version
        JToken time = obj.SelectToken("$.result").SelectToken("$.app").SelectToken("$.timestamp");
        long dateLong = (long)Convert.ToDouble(time.ToString());
        System.DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(dateLong).LocalDateTime;
        JToken version = obj.SelectToken("$.result").SelectToken("$.app").SelectToken("$.version");
        _refreshLabel.SetText("Last refreshed at: " + dateTime.ToString("H:mm:ss") + "\nGame Version: " + version.ToString());
        //services
        SetServicesStatus(obj.SelectToken("$.result").SelectToken("$.services"));
        //matchmaking
        SetMatchmakingStatus(obj.SelectToken("$.result").SelectToken("$.matchmaking"));
        //servers
        SetServersStatus(obj.SelectToken("$.result").SelectToken("$.datacenters"));

        _contentBox.SetVisible(true);
        _spinner.SetVisible(false);
    }
    private void SetServicesStatus(JToken services)
    {
        _communityLabel.SetMarkup(ColorServices(services.SelectToken("$.SessionsLogon").ToString()));
        _sessionsLabel.SetMarkup(ColorServices(services.SelectToken("$.SteamCommunity").ToString()));
        _leaderboardsLabel.SetMarkup(ColorServices(services.SelectToken("$.Leaderboards").ToString()));
    }
    private string ColorServices(string value)
    {
        if (value == "idle" || value == "normal")
        {
            return "<span color=\"" + Globals.COLOR_GREEN + "\"> ⬤ " + value + " </span>";
        }
        else
        {
            return "<span color=\"" + Globals.COLOR_RED + "\"> ⬤" + value + " </span>";
        }
    }

    private void SetMatchmakingStatus(JToken matchmaking)
    {
        string status = "Scheduler: ";
        status += ColorServices(matchmaking.SelectToken("$.scheduler").ToString());
        status += "\nOnline servers: " + matchmaking.SelectToken("$.online_servers").ToString();
        status += "\nOnline players: " + matchmaking.SelectToken("$.online_players").ToString();
        status += "\nSearching players: " + matchmaking.SelectToken("$.searching_players").ToString();
        status += "\nSearch sec avg: " + matchmaking.SelectToken("$.search_seconds_avg").ToString();
        _matchmakingLabel.SetMarkup(status);
    }
    private void SetServersStatus(JToken datacenters)
    {
        foreach (var datacenter in datacenters)
        {
            JProperty parentProp = (JProperty)datacenter;
            string serverName = parentProp.Name;
            ServerStatus serverStatus = new ServerStatus(serverName, parentProp.Value.SelectToken("$.load").ToString(), parentProp.Value.SelectToken("$.capacity").ToString());
            //europe
            if (serverName.Contains("EU") || serverName.Contains("United Kingdom"))
            {
                _boxStatus.Append(serverStatus);
            }//americas
            else if (
                serverName.Contains("US")
                || serverName.Contains("Brazil")
                || serverName.Contains("Peru")
                || serverName.Contains("Chile")
                || serverName.Contains("Argentina")
            )
            {
                _boxStatusAmericas.Append(serverStatus);
            }//asia
            else if (
                serverName.Contains("India")
                || serverName.Contains("Korea")
                || serverName.Contains("China")
                || serverName.Contains("India")
                || serverName.Contains("Korea")
                || serverName.Contains("China")
                || serverName.Contains("Japan")
                || serverName.Contains("Hong Kong")
                || serverName.Contains("Singapore")
                || serverName.Contains("Emirates")
            )
            {
                _boxStatusAsia.Append(serverStatus);
            }
            else
            {
                _boxStatusRest.Append(serverStatus);
            }
        }
    }
}
