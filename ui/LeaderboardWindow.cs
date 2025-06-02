namespace ui;
using Newtonsoft.Json.Linq;

public class LeaderboardWindow : Gtk.Box
{
    [Gtk.Connect] private readonly Gtk.Box leaderboardBox;
    [Gtk.Connect] private readonly Gtk.DropDown dropdown;
    [Gtk.Connect] private readonly Gtk.ToggleButton buttonSmallLeader;
    string fetchURL = $"https://api.steampowered.com/ICSGOServers_730/GetLeaderboardEntries/v1?format=json&lbname=official_leaderboard_premier_season2";
    private string[] regions = new string[8] { "World", "Europe", "North America", "Asia", "China", "Australia", "South America", "Africa" };
    private string title = "";
    private string subtitle = "";
    MainWindow mainWindow;
    private LeaderboardWindow(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public LeaderboardWindow(MainWindow mainWindow) : this(new Gtk.Builder("LeaderboardWindow.ui"), "leaderboard_window")
    {
        this.mainWindow = mainWindow;
        OnRealize += (sender, e) => Fetch();
        buttonSmallLeader.OnClicked += (sender, e) => Fetch((int)dropdown.GetSelected());
        OnMap += (_, _) => mainWindow.SetTitle(title, subtitle);
        SetTitle("Leaderboards", regions[0]);
        //dropdown menu
        dropdown.SetModel(Gtk.StringList.New(regions));
        dropdown.OnNotify += (sender, e) =>
        {
            int regionId = (int)dropdown.GetSelected();
            Fetch(regionId);
            SetTitle("Leaderboards", regions[regionId]);
        };
    }
    private void SetTitle(string title, string subtitle = "")
    {
        this.title = title;
        this.subtitle = subtitle;
        mainWindow.SetTitle(title, subtitle);
    }
    private void CleanChildren()
    {
        Gtk.Widget toRemove = leaderboardBox.GetLastChild();
        while (toRemove != null)
        {
            leaderboardBox.Remove(toRemove);
            toRemove = leaderboardBox.GetLastChild();
        }
    }
    private void SetLoadingScreen()
    {
        Gtk.Spinner spinner = new Gtk.Spinner();
        spinner.SetHexpand(true);
        spinner.SetVexpand(true);
        spinner.SetHalign(Gtk.Align.Center);
        spinner.SetValign(Gtk.Align.Center);
        spinner.SetSpinning(true);
        spinner.WidthRequest = 40;
        spinner.HeightRequest = 40;
        leaderboardBox.Append(spinner);
    }
    private void Fetch(int regionId = 0)
    {
        CleanChildren();
        SetLoadingScreen();
        FetchData(regionId);
    }

    private void SetData(string data)
    {
        CleanChildren();
        JObject obj = JObject.Parse(data);
        JToken players = obj.SelectToken("$.result").SelectToken("$.entries");
        int i = 1;
        if (!buttonSmallLeader.GetActive())
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
                leaderboardBox.Append(box);

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
                leaderboardBox.Append(row);
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
    private async void FetchData(int regionId = 0)
    {
        string baseURL = fetchURL;
        //add region and remove white spaces
        if (regionId != 0)
        {
            baseURL += "_" + regions[regionId].Replace(" ", "");
        }
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
