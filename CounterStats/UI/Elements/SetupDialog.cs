using CounterStats.Enums;

namespace CounterStats.UI.Elements;

public class SetupDialog : Adw.Dialog
{
    [Gtk.Connect] private readonly Gtk.Button _welcomePageButton;
    [Gtk.Connect] private readonly Gtk.Button _profilePageButton;
    [Gtk.Connect] private readonly Gtk.Button _profilePageButtonSkip;
    [Gtk.Connect] private readonly Gtk.Button _profilePageButtonBack;
    [Gtk.Connect] private readonly Gtk.Button _themePageButton;
    [Gtk.Connect] private readonly Gtk.Button _themePageButtonBack;
    [Gtk.Connect] private readonly Gtk.Button _finnishPageButton;
    [Gtk.Connect] private readonly Gtk.Button _miscPageButton;
    [Gtk.Connect] private readonly Gtk.Entry _profilePageEntry;
    [Gtk.Connect] private readonly Gtk.Entry _profilePageApiEntry;
    [Gtk.Connect] private readonly Gtk.Button _welcomePageButtonSkip;
    [Gtk.Connect] private readonly Adw.CarouselIndicatorDots _titleDots;
    [Gtk.Connect] private readonly Adw.WindowTitle _windowTitle;
    [Gtk.Connect] private readonly Adw.Carousel _carousel;
    [Gtk.Connect] private readonly Gtk.Box _themePageBox;
    [Gtk.Connect] private readonly Gtk.Label _themePageLabel;
    [Gtk.Connect] private readonly Adw.StatusPage _profilePage;
    [Gtk.Connect] private readonly Gtk.Box _profileBox;
    [Gtk.Connect] private readonly Gtk.Box _profileSkipBox;
    [Gtk.Connect] private readonly Gtk.CheckButton _profileCheck;
    [Gtk.Connect] private readonly Gtk.DropDown _miscPagecurrencyDropdown;
    [Gtk.Connect] private readonly Gtk.Switch _miscSidebarSwitch;
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
        _profilePageButton.OnClicked += (_, _) => { SetProfileDataAsync(); MoveToNextPage(); };
        _profilePageButtonSkip.OnClicked += (_, _) => { MoveToNextPage(); };
        _profilePageButtonBack.OnClicked += (_, _) => MoveToPrevPage();
        _profilePage.SetDescription(profileDesc);
        _profilePageEntry.OnNotify += (_, _) => SetProfileButton();
        _profilePageApiEntry.OnNotify += (_, _) => SetProfileButton();
        _profileCheck.OnToggled += (_, _) =>
        {
            if (_profileCheck.GetActive())
            {
                _profileBox.SetVisible(false);
                _profileSkipBox.SetVisible(true);
                _profilePageButton.SetVisible(false);
                _profilePageButtonSkip.SetVisible(true);
            }
            else
            {
                _profileBox.SetVisible(true);
                _profileSkipBox.SetVisible(false);
                _profilePageButton.SetVisible(true);
                _profilePageButtonSkip.SetVisible(false);
            }
            //  SetProfileButton();
        };
        //misc page
        _miscPagecurrencyDropdown.OnRealize += (_, _) => { PopulateCurrencyRow(); };
        _miscPageButton.OnClicked += (_, _) => { SetMiscData(); MoveToNextPage(); };
        //finish page
        _finnishPageButton.OnClicked += (_, _) => this.Close();
        //change dialog title
        _carousel.OnPageChanged += (_, e) => { SetPage((int)e.Index); };
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
    private void SetPage(int page)
    {
        if (page == 0)
        {
            _titleDots.SetVisible(false);
            _windowTitle.SetVisible(true);
        }
        else
        {
            _titleDots.SetVisible(true);
            _windowTitle.SetVisible(false);
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

    private async Task SetProfileDataAsync()
    {
        _configuration.ApiKey = apiKey;
        string data = await Globals.GetSteamID64(profilePage, apiKey);
        _configuration.SteamProfile = data;
    }

    private void PopulateCurrencyRow()
    {
        Gtk.StringList stringList = new Gtk.StringList();
        foreach (var item in Enum.GetNames<Currency>())
        {
            stringList.Append(item.ToString());

        }
        _miscPagecurrencyDropdown.SetModel(stringList);
    }

    private void SetMiscData()
    {
        _configuration.HideSidebar = _miscSidebarSwitch.GetActive();
        _configuration.Currency = (int)_miscPagecurrencyDropdown.GetSelected();
    }
}
