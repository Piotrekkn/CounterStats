using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace CounterStats;

public static class Globals
{
    /// <summary>The application ID that is used to identify your application,
    /// see https://developer.gnome.org/documentation/tutorials/application-id.html.
    /// This should be automatically replaced when the application is created.
    /// </summary>
    public const string APP_ID = "org.counterstats";

    /// <summary>
    /// A shorter name for the application.
    /// This is case sensitive and should not contain spaces.
    /// This should be automatically replaced when the application is created.
    /// </summary>
    public const string APP_SHORT_NAME = "Counter Stats";

    /// <summary>
    /// The display name of the application.
    /// This should be automatically replaced when the application is created.
    /// </summary>
    public const string APP_DISPLAY_NAME = "Counter Stats";
    /// <summary>
    /// version
    /// </summary>
    public const string VERSION = "alpha.0";
    public const string COLOR_RED = "#e62d42";
    public const string COLOR_GREEN = "#3a944a";
    public const string COLOR_BLUE = "#3584e4";
    public const string COLOR_YELLOW = "#c88800";

    public static async Task<string> FetchData(string FetchURL)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = await client.GetAsync(FetchURL))
                {
                    using (HttpContent content = res.Content)
                    {
                        string data = await content.ReadAsStringAsync();
                        return data;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            return exception.Message;
        }
    }
    /// <summary>
    /// Sets string of data as css style and applies it
    /// </summary>
    public static void SetCssData(string data, uint priority = 0)
    {
        Gtk.CssProvider cssProvider = new Gtk.CssProvider();
        cssProvider.LoadFromString(data);
        Gdk.Display display = Gdk.Display.GetDefault();
        Gtk.StyleContext.AddProviderForDisplay(display, cssProvider, priority);
    }

    public static async Task<string> GetSteamID64(string steamProfile, string apiKey = "")
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return steamProfile;
        }
        string url = "https://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key=" + apiKey + "&vanityurl=" + steamProfile;
        string data = await FetchData(url);
        if (data.Contains("\"success\":42"))
        {
            return steamProfile;
        }
        else if (data.Contains("steamid"))
        {
            JObject obj = JObject.Parse(data);
            string steamId = (string)obj["response"]?["steamid"];
            return steamId;
        }
        else
        {
            return steamProfile;
        }
    }

    public static bool IsWindows() =>
       System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

    public static bool IsLinux() =>
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
}