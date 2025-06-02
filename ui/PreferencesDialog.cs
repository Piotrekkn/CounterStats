namespace ui;

public class PreferencesDialog : Adw.PreferencesDialog
{
    [Gtk.Connect] private readonly Adw.ButtonRow clear_cache_button;
    [Gtk.Connect] private readonly Adw.ButtonRow clear_data_button;
    [Gtk.Connect] private readonly Adw.PasswordEntryRow api_entry_row;
    [Gtk.Connect] private readonly Adw.SwitchRow updates_markup_switch_row;
    [Gtk.Connect] private readonly Adw.SwitchRow clear_cache_switch;
    [Gtk.Connect] private readonly Adw.EntryRow steam_profile_entry_row;
    [Gtk.Connect] private readonly Adw.SpinRow updates_number_spin_row;
    private MainWindow mainWindow;
    private ConfigurationManager configuration;

    private PreferencesDialog(Gtk.Builder builder, string name) : base(new Adw.Internal.PreferencesDialogHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public PreferencesDialog(MainWindow mainWindow, ConfigurationManager configuration) : this(new Gtk.Builder("PreferencesDialog.ui"), "preferencesDialog")
    {
        this.configuration = configuration;
        this.mainWindow = mainWindow;
        //api entry
        api_entry_row.SetText(configuration.ApiKey);
        api_entry_row.OnNotify += (_, _) => { configuration.ApiKey = api_entry_row.GetText(); };
        //steam profile id entry
        steam_profile_entry_row.SetText(configuration.SteamProfile);
        steam_profile_entry_row.OnNotify += (_, _) => { configuration.SteamProfile = steam_profile_entry_row.GetText(); };
        //number of updates
        updates_number_spin_row.SetAdjustment(Gtk.Adjustment.New(20, 1, 50, 1, 5, 0));
        updates_number_spin_row.SetValue(configuration.UpdatesNumber);
        updates_number_spin_row.OnNotify += (_, _) => { configuration.UpdatesNumber = (int)updates_number_spin_row.GetValue(); };
        //use markup switch
        updates_markup_switch_row.SetActive(configuration.UseMarkup);
        updates_markup_switch_row.OnNotify += (_, _) => { configuration.UseMarkup = updates_markup_switch_row.GetActive(); };
        //clear chache switch
        clear_cache_switch.SetActive(configuration.ClearCacheOnQuit);
        clear_cache_switch.OnNotify += (_, _) => { configuration.ClearCacheOnQuit = clear_cache_switch.GetActive(); };
        //buttons
        clear_cache_button.OnActivated += (_, _) => configuration.ClearCache();
        clear_data_button.OnActivated += (_, _) => configuration.ClearData();
    }

}
