namespace CounterStats.UI.Elements;

public class SetupDialog : Adw.Dialog
{
    [Gtk.Connect] private readonly Gtk.Button _welcomePageButton;
    [Gtk.Connect] private readonly Gtk.Button _profilePageButton;
    [Gtk.Connect] private readonly Gtk.Button _profilePageButtonBack;
    [Gtk.Connect] private readonly Gtk.Button _themePageButton;
    [Gtk.Connect] private readonly Gtk.Button _themePageButtonBack;
    [Gtk.Connect] private readonly Gtk.Button _finnishPageButton;
    [Gtk.Connect] private readonly Gtk.Entry _profilePageEntry;
    [Gtk.Connect] private readonly Gtk.Entry _profilePageApiEntry;
    [Gtk.Connect] private readonly Gtk.Button _welcomePageButtonSkip;
    [Gtk.Connect] private readonly Adw.Carousel _carousel;
    [Gtk.Connect] private readonly Gtk.Box _themePageBox;
    [Gtk.Connect] private readonly Gtk.Label _themePageLabel;
    [Gtk.Connect] private readonly Adw.StatusPage _profilePage;
    private string profilePage, apiKey;
    private string profileDesc = "You can obtain your steam web api key at: <a href='https://steamcommunity.com/dev/apikey'>steamcommunity.com/dev/apikey</a>";
    private ConfigurationManager _configuration;

    private SetupDialog(Gtk.Builder builder, string name) : base(new Adw.Internal.DialogHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }

    public SetupDialog(ConfigurationManager configuration) : this(new Gtk.Builder("SetupDialog.ui"), "_root")
    {
        _configuration = configuration;
        //welcome page
        _welcomePageButton.OnClicked += (_, _) => MoveToNextPage();
        _welcomePageButtonSkip.OnClicked += (_, _) => this.Close();
        //theme page
        _themePageButton.OnClicked += (_, _) => MoveToNextPage();
        _themePageButtonBack.OnClicked += (_, _) => MoveToPrevPage();
        ThemeSelector themeSelector = new ThemeSelector(_configuration);
        themeSelector.AddCssClass("big-theme");
        _themePageBox.Append(themeSelector);
        Adw.StyleManager.GetDefault().OnNotify += (_, _) => SetThemeLabel();
        SetThemeLabel();
        //profile page
        _profilePageButton.OnClicked += (_, _) => { SetProfileData(); MoveToNextPage(); };
        _profilePageButtonBack.OnClicked += (_, _) => MoveToPrevPage();
        _profilePage.SetDescription(profileDesc);
        _profilePageEntry.OnNotify += (_, _) => SetProfileButton();
        _profilePageApiEntry.OnNotify += (_, _) => SetProfileButton();
        //finish page
        _finnishPageButton.OnClicked += (_, _) => this.Close();
    }

    private void MoveToNextPage()
    {
        if (_carousel.GetPosition() < _carousel.GetNPages() - 1)
        {
            _carousel.ScrollTo(_carousel.GetNthPage((uint)_carousel.GetPosition() + 1), true);
        }
    }

    private void MoveToPrevPage()
    {
        if (_carousel.GetPosition() > 0)
        {
            _carousel.ScrollTo(_carousel.GetNthPage((uint)_carousel.GetPosition() - 1), true);
        }
    }

    private void SetThemeLabel()
    {
        switch (Adw.StyleManager.GetDefault().GetColorScheme())
        {
            case Adw.ColorScheme.Default:
                _themePageLabel.SetText("Follow System Settings");
                break;
            case Adw.ColorScheme.ForceDark:
                _themePageLabel.SetText("Dark Theme");
                break;
            case Adw.ColorScheme.ForceLight:
                _themePageLabel.SetText("Light Theme");
                break;
            default:
                _themePageLabel.SetText("");
                break;
        }
    }

    private void SetProfileButton()
    {
        //make sure the data has changed
        if (profilePage == _profilePageEntry.GetText() && apiKey == _profilePageApiEntry.GetText())
        {
            return;
        }
        else
        {
            profilePage = _profilePageEntry.GetText();
            apiKey = _profilePageApiEntry.GetText();
        }
        bool emptyApi = string.IsNullOrEmpty(apiKey);
        bool emptyProfile = string.IsNullOrEmpty(profilePage);
        if (!emptyApi && !emptyProfile)
        {
            _profilePageButton.SetSensitive(true);
        }
        else
        {
            _profilePageButton.SetSensitive(false);
        }
        //color entries if they are empty
        if (emptyProfile)
        {
            _profilePageEntry.SetCssClasses(["error"]);
        }
        else
        {
            _profilePageEntry.SetCssClasses(["regular"]);
        }
        if (emptyApi)
        {
            _profilePageApiEntry.SetCssClasses(["error"]);
        }
        else
        {
            _profilePageApiEntry.SetCssClasses(["regular"]);
        }
    }

    private void SetProfileData()
    {
        _configuration.SteamProfile = profilePage;
        _configuration.ApiKey = apiKey;
    }
}
