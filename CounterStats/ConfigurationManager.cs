namespace CounterStats;

using System.Runtime.ConstrainedExecution;
using System.Text.Json;
using CounterStats.UI;
using CounterStats.UI.Elements;
public class ConfigurationManager
{
    [Serializable]
    class Data
    {
        public string SteamProfile { get; set; }
        public string ApiKey { get; set; }
        public bool UseMarkup { get; set; }
        public int UpdatesNumber { get; set; }
        public bool ClearCacheOnQuit { get; set; }
        public int DefaultWindow { get; set; }
        public int ItemsNumber { get; set; }
        public bool AutoFetchPrices { get; set; }
        public int Currency { get; set; }
        public bool HideSidebar { get; set; }
        public int CurrentTheme { get; set; }
    }

    public string ApiKey
    {
        get { return apiKey; }
        set
        {
            if (apiKey != value)
            {
                apiKey = value;
                Save();
            }
        }
    }
    private string apiKey;
    public string SteamProfile
    {
        get { return steamProfile; }
        set
        {
            if (steamProfile != value)
            {
                steamProfile = value;
                Save();
            }
        }
    }
    private string steamProfile;
    public bool UseMarkup
    {
        get { return useMarkup; }
        set
        {
            if (useMarkup != value)
            {
                useMarkup = value;
                Save();
            }
        }
    }
    private bool useMarkup;
    public int UpdatesNumber
    {
        get { return updatesNumber; }
        set
        {
            if (updatesNumber != value)
            {
                updatesNumber = value;
                Save();
            }
        }
    }
    private int updatesNumber;
    public bool ClearCacheOnQuit
    {
        get { return clearCacheOnQuit; }
        set
        {
            if (clearCacheOnQuit != value)
            {
                clearCacheOnQuit = value;
                Save();
            }
        }
    }
    private bool clearCacheOnQuit;
    public int DefaultWindow
    {
        get { return defaultWindow; }
        set
        {
            if (defaultWindow != value)
            {
                defaultWindow = value;
                Save();
            }
        }
    }
    private int defaultWindow;
    public int ItemsNumber
    {
        get { return itemsNumber; }
        set
        {
            if (itemsNumber != value)
            {
                itemsNumber = value;
                Save();
            }
        }
    }
    private int itemsNumber;
    public bool AutoFetchPrices
    {
        get { return autoFetchPrices; }
        set
        {
            if (autoFetchPrices != value)
            {
                autoFetchPrices = value;
                Save();
            }
        }
    }
    private bool autoFetchPrices;
    public int Currency
    {
        get { return currency; }
        set
        {
            if (currency != value)
            {
                currency = value;
                Save();
            }
        }
    }
    private int currency;
    public bool HideSidebar
    {
        get { return hideSidebar; }
        set
        {
            if (hideSidebar != value)
            {
                hideSidebar = value;
                Save();
            }
        }
    }
    private bool hideSidebar;
    public int CurrentTheme
    {
        get { return currentTheme; }
        set
        {
            if (currentTheme != value)
            {
                currentTheme = value;
                Save();
            }
        }
    }
    private int currentTheme;
    private MainApp _mainApp;
    private string cacheDir =
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.cache/counterstats/";
    private string configDir =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/counterstats/";
    private string configFile = "data.json";
    public bool firstRun { get; private set; }

    public ConfigurationManager(MainApp mainApp)
    {
        _mainApp = mainApp;
        Load();
    }

    private void Save()
    {
        Data data = new Data
        {
            UseMarkup = this.UseMarkup,
            ApiKey = this.ApiKey,
            UpdatesNumber = this.UpdatesNumber,
            ClearCacheOnQuit = this.ClearCacheOnQuit,
            SteamProfile = this.SteamProfile,
            DefaultWindow = this.DefaultWindow,
            ItemsNumber = this.ItemsNumber,
            AutoFetchPrices = this.AutoFetchPrices,
            Currency = this.Currency,
            HideSidebar = this.HideSidebar,
            CurrentTheme = this.CurrentTheme,
        };
        string jsonString = JsonSerializer.Serialize(data);
        File.WriteAllText(configDir + configFile, jsonString);
    }

    private void Load()
    {
        System.IO.Directory.CreateDirectory(cacheDir);
        System.IO.Directory.CreateDirectory(configDir);
        string jsonData = null;
        if (File.Exists(configDir + configFile))
        {
            jsonData = File.ReadAllText(configDir + configFile);
        }
        if (jsonData != null)
        {
            firstRun = false;
            Data data = JsonSerializer.Deserialize<Data>(jsonData);
            UseMarkup = data.UseMarkup;
            ApiKey = data.ApiKey;
            UpdatesNumber = data.UpdatesNumber;
            ClearCacheOnQuit = data.ClearCacheOnQuit;
            SteamProfile = data.SteamProfile;
            DefaultWindow = data.DefaultWindow;
            ItemsNumber = data.ItemsNumber;
            AutoFetchPrices = data.AutoFetchPrices;
            Currency = data.Currency;
            HideSidebar = data.HideSidebar;
            CurrentTheme = data.CurrentTheme;
        }
        else
        {
            firstRun = true;
            NewData();
        }
    }

    private void NewData()
    {
        FirstTimeSetup();
        if (string.IsNullOrEmpty(ApiKey))
            ApiKey = "";
        UseMarkup = true;
        if (string.IsNullOrEmpty(SteamProfile))
            SteamProfile = "";
        UpdatesNumber = 20;
        ClearCacheOnQuit = false;
        DefaultWindow = 0;
        ItemsNumber = 300;
        AutoFetchPrices = false;
        Currency = 0;
        HideSidebar = false;
        CurrentTheme = 0;
    }

    public void FirstTimeSetup()
    {
        SetupDialog setup = new SetupDialog(this);
        setup.Present(_mainApp);
        setup.OnClosed += (_, _) =>
        {
            firstRun = false;
            //setdata
            _mainApp.ReloadWindow();
        };
    }

    public void ClearCache()
    {
        //create and display dialog
        Adw.AlertDialog alertDialog = new Adw.AlertDialog();
        alertDialog.AddResponse("cancel", "No");
        alertDialog.AddResponse("confirm", "Yes");
        alertDialog.SetResponseAppearance("confirm", Adw.ResponseAppearance.Destructive);
        alertDialog.SetResponseAppearance("cancel", Adw.ResponseAppearance.Default);
        alertDialog.SetHeading("Do you want to delete cache data?");
        alertDialog.SetBody("This action will delete all the temporary files and cache data!");
        alertDialog.Present(_mainApp);
        //handle the response
        alertDialog.OnResponse += (_, response) =>
        {
            if (response.Response == "confirm")
            {
                Console.WriteLine("cleaning cache");
                DeleteDirectory(cacheDir);
            }
        };
    }

    public void ClearData()
    {
        //create and display dialog
        Adw.AlertDialog alertDialog = new Adw.AlertDialog();
        alertDialog.AddResponse("cancel", "No");
        alertDialog.AddResponse("confirm", "Yes");
        alertDialog.SetResponseAppearance("confirm", Adw.ResponseAppearance.Destructive);
        alertDialog.SetResponseAppearance("cancel", Adw.ResponseAppearance.Default);
        alertDialog.SetHeading("Do you want to delete all of the configuration data?");
        alertDialog.SetBody(
            "This action will delete all the configuration data (including apis and logins), temporary files and cache data!. Application will be closed"
        );
        alertDialog.Present(_mainApp);
        //handle the response
        alertDialog.OnResponse += (_, res) =>
        {
            if (res.Response == "confirm")
            {
                Console.WriteLine("cleaning cache");
                DeleteDirectory(cacheDir);
                Console.WriteLine("cleaning config");
                DeleteDirectory(configDir);
                _mainApp.Application.Quit();
            }
        };
    }

    private void DeleteDirectory(string dir)
    {
        if (!System.IO.Directory.Exists(dir))
        {
            return;
        }
        //remove all the files in the cache directory
        var files = System.IO.Directory.EnumerateFiles(dir);
        if (files != null)
        {
            foreach (var file in files)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
        }
        //remove the directory
        try
        {
            System.IO.Directory.Delete(dir);
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}
