
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
                if (File.Exists(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/icons.gresource"))
                {
                    //Load file from program directory, required for `dotnet run`
                    Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/icons.gresource"));            
                }            
                //Add icons
                var theme = Gtk.IconTheme.GetForDisplay(Gdk.Display.GetDefault());
                theme.AddResourcePath("/org/counterstats/icons");
                // Create a new MainApp and show it.
                // The application is passed to the MainApp so that it can be used
                var MainApp = new MainApp((Adw.Application)application);  
                MainApp.Show();               
            };
        app.RunWithSynchronizationContext(null);
    }

}

