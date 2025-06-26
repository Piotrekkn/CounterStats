using CounterStats.Enums;
using Gio;
using GLib.Internal;

namespace CounterStats.UI.Elements;

public class PreferencesDialog : Adw.PreferencesDialog
{
    [Gtk.Connect] private readonly Adw.ButtonRow _clearCacheButton;
    [Gtk.Connect] private readonly Adw.ButtonRow _clearDataButton;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow _apiEntryRow;
    [Gtk.Connect] private readonly Adw.SwitchRow _updatesMarkupSwitchRow;
    [Gtk.Connect] private readonly Adw.SwitchRow _clearCacheSwitch;
    [Gtk.Connect] private readonly Adw.EntryRow _steamProfileEntryRow;
    [Gtk.Connect] private readonly Adw.SpinRow _updatesNumberSpinRow;
    [Gtk.Connect] private readonly Adw.ComboRow _windowComboRow;
    [Gtk.Connect] private readonly Adw.SpinRow _inventoryNumberSpinRow;
    [Gtk.Connect] private readonly Adw.ActionRow _apiActionRow;
    [Gtk.Connect] private readonly Adw.SwitchRow _sidebarHideRow;
    [Gtk.Connect] private readonly Adw.SwitchRow _inventoryAutoPriceSwitchRow;
    [Gtk.Connect] private readonly Adw.PreferencesPage _apiPage;
    [Gtk.Connect] private readonly Adw.ComboRow _currencyComboRow;
    [Gtk.Connect] private readonly Adw.ButtonRow _setupButton;
    private ConfigurationManager _configuration;

    private PreferencesDialog(Gtk.Builder builder, string name) : base(new Adw.Internal.PreferencesDialogHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public PreferencesDialog(ConfigurationManager configuration, string[] windowList) : this(new Gtk.Builder("PreferencesDialog.ui"), "_root")
    {
        _configuration = configuration;
        //api entry
        _apiEntryRow.SetText(configuration.ApiKey);
        _apiEntryRow.OnNotify += (_, _) => { configuration.ApiKey = _apiEntryRow.GetText(); SetApiIcon(); };
        //add url
        _apiActionRow.SetUseMarkup(true);
        _apiActionRow.SetSubtitle("You can obtain your steam web api key at: <a href='https://steamcommunity.com/dev/apikey'>steamcommunity.com/dev/apikey</a>");
        //steam profile id entry
        _steamProfileEntryRow.SetText(configuration.SteamProfile);
        _steamProfileEntryRow.OnStateFlagsChanged += (_, _) => { SetProfileDataAsync(); };
        //number of updates
        _updatesNumberSpinRow.SetAdjustment(Gtk.Adjustment.New(20, 1, 50, 1, 5, 0));
        _updatesNumberSpinRow.SetValue(configuration.UpdatesNumber);
        _updatesNumberSpinRow.OnNotify += (_, _) => { configuration.UpdatesNumber = (int)_updatesNumberSpinRow.GetValue(); };
        //use markup switch
        _updatesMarkupSwitchRow.SetActive(configuration.UseMarkup);
        _updatesMarkupSwitchRow.OnNotify += (_, _) => { configuration.UseMarkup = _updatesMarkupSwitchRow.GetActive(); };
        //clear chache switch
        _clearCacheSwitch.SetActive(configuration.ClearCacheOnQuit);
        _clearCacheSwitch.OnNotify += (_, _) => { configuration.ClearCacheOnQuit = _clearCacheSwitch.GetActive(); };
        //buttons
        _clearCacheButton.OnActivated += (_, _) => configuration.ClearCache();
        _clearDataButton.OnActivated += (_, _) => configuration.ClearData();
        _setupButton.OnActivated += (_, _) => { this.Close(); configuration.FirstTimeSetup(); };
        //default window combo row
        _windowComboRow.OnRealize += (_, _) => { PopulateWindowRow(windowList, configuration.DefaultWindow); };
        _windowComboRow.OnNotify += (_, _) => { configuration.DefaultWindow = (int)_windowComboRow.GetSelected(); };
        //hide sidebar switch row
        _sidebarHideRow.SetActive(configuration.HideSidebar);
        _sidebarHideRow.OnNotify += (_, _) => { configuration.HideSidebar = _sidebarHideRow.GetActive(); };
        //inventory number of items
        _inventoryNumberSpinRow.SetAdjustment(Gtk.Adjustment.New(1000, 50, 5000, 50, 100, 0));
        _inventoryNumberSpinRow.SetValue(configuration.ItemsNumber);
        _inventoryNumberSpinRow.OnNotify += (_, _) => { configuration.ItemsNumber = (int)_inventoryNumberSpinRow.GetValue(); };
        //inventory auto fetch prices
        _inventoryAutoPriceSwitchRow.SetActive(configuration.AutoFetchPrices);
        _inventoryAutoPriceSwitchRow.OnNotify += (_, _) => { configuration.AutoFetchPrices = _inventoryAutoPriceSwitchRow.GetActive(); };
        //make sure api has set icon correctly
        _inventoryAutoPriceSwitchRow.OnRealize += (_, _) => { SetApiIcon(); };
        //currency row
        _currencyComboRow.OnRealize += (_, _) => { PopulateCurrencyRow(); };
    }

    private void SetApiIcon()
    {
        //check if api and prfile rows are populated
        if (!string.IsNullOrEmpty(_configuration.ApiKey) && !string.IsNullOrEmpty(_configuration.SteamProfile))
        {
            _apiPage.SetIconName("weather-overcast-symbolic");
        }
        else
        {
            _apiPage.SetIconName("weather-severe-alert-symbolic");
        }
    }

    private void PopulateWindowRow(string[] windowList, int defaultWindow)
    {
        Gtk.StringList stringList = new Gtk.StringList();
        foreach (var item in windowList)
        {
            stringList.Append(item);
        }
        _windowComboRow.SetModel(stringList);
        _windowComboRow.SetSelected((uint)defaultWindow);
    }

    private void PopulateCurrencyRow()
    {
        Gtk.StringList stringList = new Gtk.StringList();
        foreach (var item in Enum.GetNames<Currency>())
        {
            stringList.Append(item.ToString());

        }
        _currencyComboRow.SetModel(stringList);
        _currencyComboRow.SetSelected((uint)_configuration.Currency);
        _currencyComboRow.OnNotify += (_, _) => { _configuration.Currency = (int)_currencyComboRow.GetSelected(); };
    }
    private async System.Threading.Tasks.Task SetProfileDataAsync()
    {
        string data = await Globals.GetSteamID64(_steamProfileEntryRow.GetText(), _configuration.ApiKey);
        _configuration.SteamProfile = data;
        SetApiIcon();
    }
}
