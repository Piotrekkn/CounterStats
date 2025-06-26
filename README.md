# **Counter Stats**
![](screenshots/logo.png)

> Display Counter Strike 2 statistics, inventory, leaderboards and much more, all in one app!


## Gallery
|![Profile](screenshots/1.png)Profile|![ServerStatus](screenshots/2.png)Server status|![inventory](screenshots/3.png)Inventory|
|--------------------|--------------------|--------------------|
|![preferences](screenshots/4.png)Preferences|![leaderboards](screenshots/5.png)Leaderboards|![ThemeChooser](screenshots/6.png)Initial setup Themepicker|
|![statisticsWindows](screenshots/7.png)Statistics (Windows)|![inentoryWindows](screenshots/8.png)Inventory (Windows)|![gameUpdates](screenshots/9.png)Game Updates (Windows)|


## Build

### Linux

#### Running


To run this project, run the following commands:

```bash
dotnet run
```

#### Building

To build and install flatpak just run build_flatpak.sh:

```bash
./build_flatpak.sh
```

### Windows
Make sure to setup gtk and dotnet sdk first:

[Dotnet SDK](https://dotnet.microsoft.com/en-us/download)

[Gtk Install](https://www.gtk.org/docs/installations/windows/)

MSYS2 dependencies:

```bash
pacman -S mingw-w64-x86_64-gtk4 mingw-w64-x86_64-libadwaita mingw-w64-x86_64-blueprint-compiler mingw-w64-x86_64-librsvg 
```

#### Running

To run this project, run the following commands:

```bash
dotnet run
```

#### Building
To build and install flatpak just run build_windows.bat

>make sure "mingw_dir=" is set correctly (set by default to "C:\\msys64\\mingw64") in the script

Run: 

```bash
./build_windows.bat
```

## Built with:
[Gtk](https://www.gtk.org/)

[Adwaita](https://gnome.pages.gitlab.gnome.org/libadwaita/)

[Blueprints](https://gnome-team.pages.debian.net/blueprint-compiler/index.html)

[Dotnet](https://dotnet.microsoft.com/en-us/)