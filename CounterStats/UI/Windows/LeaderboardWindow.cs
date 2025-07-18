namespace CounterStats.UI.Windows;

using Newtonsoft.Json.Linq;

public class LeaderboardWindow : Gtk.Box, IWindow
{
    [Gtk.Connect] private readonly Gtk.Box _leaderboardBox;
    [Gtk.Connect] private readonly Gtk.DropDown _dropdown;
    [Gtk.Connect] private readonly Gtk.ToggleButton _buttonSmallLeader;
    public string WindowName { get; }
    public string IconName { get; }
    string fetchURL = $"https://api.steampowered.com/ICSGOServers_730/GetLeaderboardEntries/v1?format=json&lbname=official_leaderboard_premier_season2";
    private string[] regions = ["World", "Europe", "North America", "Asia", "China", "Australia", "South America", "Africa"];
    private string title = "";
    private string subtitle = "";
    MainApp _mainApp;
    private LeaderboardWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public LeaderboardWindow(MainApp mainApp, string windowName, string iconName) : this(new Gtk.Builder("LeaderboardWindow.ui"), "_root")
    {
        _mainApp = mainApp;
        WindowName = windowName;
        IconName = iconName;
        OnRealize += (sender, e) => Refresh();
        _buttonSmallLeader.OnClicked += (sender, e) => Refresh();
        OnMap += (_, _) => mainApp.SetTitle(title, subtitle);
        SetTitle("Leaderboards", regions[0]);
        //dropdown menu
        _dropdown.SetModel(Gtk.StringList.New(regions));
        _dropdown.OnNotify += (sender, e) =>
        {
            int regionId = (int)_dropdown.GetSelected();
            Fetch(regionId);
            SetTitle("Leaderboards", regions[regionId]);
        };
    }
    private void SetTitle(string title, string subtitle = "")
    {
        this.title = title;
        this.subtitle = subtitle;
        _mainApp.SetTitle(title, subtitle);
    }
    public void CleanChildren()
    {
        Gtk.Widget toRemove = _leaderboardBox.GetLastChild();
        while (toRemove != null)
        {
            _leaderboardBox.Remove(toRemove);
            toRemove = _leaderboardBox.GetLastChild();
        }
    }
    private void SetLoadingScreen()
    {
        Adw.Spinner spinner = new Adw.Spinner();
        spinner.SetHexpand(true);
        spinner.SetVexpand(true);
        _leaderboardBox.Append(spinner);
    }
    public void Refresh()
    {
        Fetch((int)_dropdown.GetSelected());
    }
    private void Fetch(int regionId = 0)
    {
        CleanChildren();
        SetLoadingScreen();
        SetDataAsync(regionId);
    }
    private async Task SetDataAsync(int regionId)
    {
        string url = fetchURL;
        //add region and remove white spaces
        if (regionId != 0)
        {
            url += "_" + regions[regionId].Replace(" ", "");
        }
        string data = await Globals.FetchData(url);
        SetData(data);
    }

    private void SetData(string data)
    {
        CleanChildren();
        JObject obj = JObject.Parse(data);
        JToken players = obj.SelectToken("$.result").SelectToken("$.entries");
        int i = 1;
        if (!_buttonSmallLeader.GetActive())
        {
            //big leaderboards
            foreach (var player in players)
            {
                Gtk.Box box = new Gtk.Box();
                box.SetMarginBottom(40);
                box.SetOrientation(Gtk.Orientation.Horizontal);
                Gtk.Box boxLeft = new Gtk.Box();
                boxLeft.SetCssClasses(["opaque"]);
                boxLeft.SetMarginEnd(20);
                Gtk.Box boxRight = new Gtk.Box();
                boxRight.SetOrientation(Gtk.Orientation.Vertical);
                box.Append(boxLeft);
                box.Append(boxRight);
                Gtk.Label labelNumber = new Gtk.Label();
                labelNumber.SetText(i++.ToString());
                labelNumber.SetCssClasses(["title-1"]);
                boxLeft.Append(labelNumber);
                Gtk.Label labelName = new Gtk.Label();
                labelName.SetUseMarkup(false);
                labelName.SetText(player.SelectToken("$.name").ToString());
                labelName.SetCssClasses(["title-2"]);
                labelName.SetXalign(0);
                boxRight.Append(labelName);
                Gtk.Label labelScore = new Gtk.Label();
                labelScore.SetUseMarkup(true);
                labelScore.SetMarkup(ColorScore((int)player.SelectToken("$.score")));
                labelScore.SetCssClasses(["title-3"]);
                labelScore.SetXalign(0);
                boxRight.Append(labelScore);
                _leaderboardBox.Append(box);
            }
        }
        else
        {
            //small leaderboards
            foreach (var player in players)
            {
                Adw.ActionRow row = new Adw.ActionRow();
                row.UseMarkup = false;
                row.SetUseMarkup(false);
                row.Title = i++.ToString() + " " + player.SelectToken("$.name").ToString();
                int score = (int)player.SelectToken("$.score");
                score = score >> 15;
                row.SetUseMarkup(true);
                row.Subtitle = ColorScore((int)player.SelectToken("$.score"));
                _leaderboardBox.Append(row);
            }
        }
    }

    private string ColorScore(int score)
    {
        score = score >> 15;
        string color;
        if (score > 30000)
        { color = "#f1ae35"; }
        else if (score > 25000)
        { color = "#eb4c4b"; }
        else if (score > 20000)
        { color = "#b12fc1"; }
        else if (score > 15000)
        { color = "#8845ff"; }
        else if (score > 10000)
        { color = "#4b69fe"; }
        else if (score > 5000)
        { color = "#5e98d9"; }
        else
        { color = "#b0c3da"; }
        return "<span color=\"" + color + "\">" + score.ToString() + "</span>";
    }
}
