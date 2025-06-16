using Gtk;

namespace CounterStats.UI.Elements;

public class ThemeSelector : Adw.Bin
{
  private string css =
      @"
.themeselector checkbutton {
   min-height: 44px;
  min-width: 44px; 
  background-clip: content-box;
  border-radius: 9999px;
  box-shadow: inset 0 0 0 1px @borders;
}
.themeselector checkbutton.follow:checked,
.themeselector checkbutton.light:checked,
.themeselector checkbutton.dark:checked {
  box-shadow: inset 0 0 0 2px @theme_selected_bg_color;
}
.themeselector checkbutton.follow {
  background-image: linear-gradient(
    to bottom right,
    #fff 49.99%,
    #202020 50.01%
  );
}
.themeselector checkbutton.light {
  background-color: #fff;
}
.themeselector checkbutton.dark { background-color: #202020;
}
.themeselector radio{
   -gtk-icon-source: none;
  border: 0;
  background: none;
  box-shadow: none;
  min-width: 12px;
  min-height: 12px;
  transform: translate(27px, 14px);
  padding: 2px;
  hidden: true;}
.themeselector radio:checked { 
  -gtk-icon-source: -gtk-icontheme('object-select-symbolic');
  background-color: @theme_selected_bg_color;
   color: var(--accent-fg-color);}
  ";
  [Gtk.Connect] private readonly Gtk.CheckButton follow;
  [Gtk.Connect] private readonly Gtk.CheckButton dark;
  [Gtk.Connect] private readonly Gtk.CheckButton light;
  private ConfigurationManager _configurationManager;

  private ThemeSelector(Gtk.Builder builder, string name) : base(new Adw.Internal.BinHandle(builder.GetPointer(name), false))
  {
    builder.Connect(this);
  }

  public ThemeSelector(ConfigurationManager configurationManager) : this(new Gtk.Builder("ThemeSelector.ui"), "_root")
  {
    _configurationManager = configurationManager;


    SetTheme(configurationManager.CurrentTheme, true);
    dark.OnToggled += (_, _) => { SetTheme(2); };
    light.OnToggled += (_, _) => { SetTheme(1); };
    follow.OnToggled += (_, _) => { SetTheme(); };
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
        light.SetActive(true);
        follow.SetActive(false);
        dark.SetActive(false);
        break;
      case 2:
        Adw.StyleManager.GetDefault().SetColorScheme(Adw.ColorScheme.ForceDark);
        dark.SetActive(true);
        follow.SetActive(false);
        light.SetActive(false);
        break;
      case 0:
      default:
        Adw.StyleManager.GetDefault().SetColorScheme(Adw.ColorScheme.Default);
        follow.SetActive(true);
        dark.SetActive(false);
        light.SetActive(false);
        break;
    }
    Gtk.CssProvider cssProvider = new Gtk.CssProvider();
    cssProvider.LoadFromString(css);
    Gdk.Display display = Gdk.Display.GetDefault();
    Gtk.StyleContext.AddProviderForDisplay(display, cssProvider, 1001010);
  }
}
