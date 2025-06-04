// This will make all the constants available in the global namespace, 
// so you can use them without the Constants prefix.
global using static Constants;
using System.Reflection;

var app = Adw.Application.New(APP_ID, Gio.ApplicationFlags.DefaultFlags);

app.OnActivate += (application, args) =>
{
    if (File.Exists(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/icons.gresource"))
    {
        //Load file from program directory, required for `dotnet run`
        Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/icons.gresource"));
    }
    else
    {

    }
    //add icons
    var theme = Gtk.IconTheme.GetForDisplay(Gdk.Display.GetDefault());
    theme.AddResourcePath("/org/counterstats/icons");
    // Create a new MainWindow and show it.
    // The application is passed to the MainWindow so that it can be used
    var mainWindow = new ui.MainWindow((Adw.Application)application);
    // string sAttr = .AppSettings.Get("Key0");
    mainWindow.Show();
    // mainWindow.Fetch();
};


// Run the application
return app.RunWithSynchronizationContext(null);
