
namespace CounterStats;

using System.Reflection;
using CounterStats.UI;
internal class Program
{
    static void Main(string[] args)
    {
        var app = Adw.Application.New(Globals.APP_ID, Gio.ApplicationFlags.DefaultFlags);
        app.OnActivate += (application, args) =>
            {
                try
                {
                    if (File.Exists(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!) + "/icons.gresource"))
                    {
                        //Load file from program directory, required for `dotnet run`
                        Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!) + "/icons.gresource"));
                    }
                    else
                    {
                        var prefixes = new List<string> {
                    Directory.GetParent(Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!))!.FullName)!.FullName,
                    Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!))!.FullName,
                    "/usr"
                };
                        foreach (var prefix in prefixes)
                        {
                            if (File.Exists(prefix + "/share/CounterStats/icons.gresource"))
                            {
                                Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(prefix + "/share/CounterStats/icons.gresource")));
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                //Add icons
                var theme = Gtk.IconTheme.GetForDisplay(Gdk.Display.GetDefault());
                theme.AddResourcePath("/org/counterstats/icons");
                // Create a new MainApp and show it.
                // The application is passed to the MainApp so that it can be used
                var MainApp = new MainApp((Adw.Application)application);
                //if windows change font size to match default adwaita style
                if (Globals.IsWindows())
                {
                    Gtk.Settings.GetDefault().GtkFontName = "Segoe UI 12";
                    Gtk.Settings.GetDefault().GtkFontRendering = Gtk.FontRendering.Automatic;
                    Gtk.Settings.GetDefault().GtkHintFontMetrics = true;
                }
                MainApp.Show();
            };
        app.RunWithSynchronizationContext(null);
    }

}

