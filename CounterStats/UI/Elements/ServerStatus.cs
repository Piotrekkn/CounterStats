namespace CounterStats.UI.Elements;

public class ServerStatus : Gtk.Box
{
    [Gtk.Connect] private readonly Gtk.Label _titleLabel;
    [Gtk.Connect] private readonly Gtk.Label _capLabel;
    [Gtk.Connect] private readonly Gtk.Label _loadLabel;
    [Gtk.Connect] private readonly Gtk.Label _flagLabel;
    private int flagSize = 78;
    public string Name { get; }
    private ServerStatus(Gtk.Builder builder, string name) : base(new Gtk.Internal.BoxHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }

    public ServerStatus(string name, string load, string capacity) : this(new Gtk.Builder("ServerStatus.ui"), "_root")
    {
        Name = name;
        _titleLabel.SetText(name);
        _loadLabel.SetMarkup(ColorLoad(load));
        _capLabel.SetMarkup(ColorCapacity(capacity));
        _flagLabel.SetMarkup(GetFlag(name));
        string cssData = ".flagLabel { font-size: " + flagSize + "px;}";
        Globals.SetCssData(cssData);
        _flagLabel.AddCssClass("flagLabel");
    }
    private string ColorLoad(string load)
    {
        string prefix = "Load: ";
        switch (load)
        {
            case "idle":
                return prefix + "<span color=\"" + Globals.COLOR_BLUE + "\"> ⬤ Idle </span>";
            case "low":
                return prefix + "<span color=\"" + Globals.COLOR_GREEN + "\"> ⬤ Low </span>";
            case "medium":
                return prefix + "<span color=\"" + Globals.COLOR_YELLOW + "\"> ⬤ Medium </span>";
            case "high":
                return prefix + "<span color=\"" + Globals.COLOR_RED + "\"> ⬤ High </span>";
            default:
                return prefix + load;
        }
    }

    private string ColorCapacity(string capacity)
    {
        string prefix = "Capacity: ";
        return prefix + capacity;
    }

    private string GetFlag(string serverName)
    {
        switch (serverName)
        {
            case "EU Poland":
                return "🇵🇱";
            case "Peru":
                return "🇵🇪";
            case "EU Austria":
                return "🇦🇹";
            case "EU Germany":
                return "🇩🇪";
            case "Hong Kong":
                return "🇭🇰";
            case "EU Spain":
                return "🇪🇸";
            case "Chile":
                return "🇨🇱";
            case "US California":
                return "🐻";
            case "US Atlanta":
            case "US Seattle":
            case "US Virginia":
            case "US Chicago":
            case "US Dallas":
                return "🇺🇸";
            case "China Guangdong":
            case "China Pudong":
            case "China Beijing":
            case "China Chengdu":
                return "🇨🇳";
            case "EU Sweden":
                return "🇸🇪";
            case "Emirates":
                return "🇦🇪";
            case "South Africa":
                return "🇿🇦";
            case "Brazil":
                return "🇧🇷";
            case "Japan":
                return "🇯🇵";
            case "EU Finland":
                return "🇫🇮";
            case "India Mumbai":
            case "India Chennai":
                return "🇮🇳";
            case "Argentina":
                return "🇦🇷";
            case "South Korea":
                return "🇰🇷";
            case "United Kingdom":
                return "🇬🇧";
            case "Singapore":
                return "🇸🇬";
            case "Australia":
                return "🇦🇺";
            default:
                return serverName;
        }
    }
}
