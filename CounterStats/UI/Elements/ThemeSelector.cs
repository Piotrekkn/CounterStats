using System.ComponentModel;
using Gtk;

namespace CounterStats.UI.Elements;

public class ThemeSelector : Adw.Bin
{
  private string css =
@".theme-selector checkbutton {
  min-height: 44px;
  min-width: 44px; 
  background-clip: content-box;
  border-radius: 9999px;
  box-shadow: inset 0 0 0 1px @borders;
}
.theme-selector checkbutton:checked {
  box-shadow: inset 0 0 0 2px @theme_selected_bg_color;
}
.theme-selector checkbutton.follow {
  background-image: linear-gradient(
    to bottom right,
    #fff 49.99%,
    #202020 50.01%
  );
}
.theme-selector checkbutton.light {
  background-color: #fff;
}
.theme-selector checkbutton.dark { 
background-color: #202020;
}
.theme-selector radio{
   -gtk-icon-source: none;
  border: 0;
  background: none;
  box-shadow: none;
  min-width: 12px;
  min-height: 12px;
  transform: translate(27px, 14px);
  padding: 2px;
  }
.theme-selector radio:checked { 
  -gtk-icon-source: -gtk-icontheme('object-select-symbolic');
  background-color: @theme_selected_bg_color;
   color: var(--accent-fg-color);
   }
   .big-theme checkbutton {
  min-height: 88px;
  min-width: 88px; 
  box-shadow: inset 0 0 0 4px @borders;
}
.big-theme radio{
  min-width: 24px;
  min-height: 24px;
  transform: translate(54px, 28px);
  padding: 8px;
  }
  .big-theme  checkbutton:checked {
  box-shadow: inset 0 0 0 4px @theme_selected_bg_color;
  }
   ";
  [Gtk.Connect] private readonly Gtk.CheckButton _followButton;
  [Gtk.Connect] private readonly Gtk.CheckButton _darkButton;
  [Gtk.Connect] private readonly Gtk.CheckButton _lightButton;
  private ConfigurationManager _configurationManager;

  private ThemeSelector(Gtk.Builder builder, string name) : base(new Adw.Internal.BinHandle(builder.GetPointer(name), false))
  {
    builder.Connect(this);
  }

  public ThemeSelector(ConfigurationManager configurationManager) : this(new Gtk.Builder("ThemeSelector.ui"), "_root")
  {
    _configurationManager = configurationManager;
    _darkButton.OnToggled += (_, _) => { SetTheme(2); };
    _lightButton.OnToggled += (_, _) => { SetTheme(1); };
    _followButton.OnToggled += (_, _) => { SetTheme(); };
    Globals.SetCssData(css, 1001010);
    OnMap += (_, _) => { Refresh(); };
  }

  private void Refresh()
  {
    SetTheme(_configurationManager.CurrentTheme, true);
  }

  private void SetTheme(int theme = 0, bool firstRun = false)
  {
    if (!firstRun && _configurationManager.CurrentTheme == theme)
    {
      return;
    }
    _configurationManager.CurrentTheme = theme;
    switch (theme)
    {
      case 1:
        Adw.StyleManager.GetDefault().SetColorScheme(Adw.ColorScheme.ForceLight);

        _followButton.Active = false;
        _darkButton.Active = false;
        _lightButton.SetActive(true);
        break;
      case 2:
        Adw.StyleManager.GetDefault().SetColorScheme(Adw.ColorScheme.ForceDark);

        _followButton.Active = false;
        _lightButton.Active = false;
        _darkButton.SetActive(true);
        break;
      case 0:
      default:
        Adw.StyleManager.GetDefault().SetColorScheme(Adw.ColorScheme.Default);
        _darkButton.Active = false;
        _lightButton.Active = false;
        _followButton.SetActive(true);
        break;
    }
  }
}
