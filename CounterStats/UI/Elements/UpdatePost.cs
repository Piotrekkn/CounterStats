namespace CounterStats.UI.Elements;

public class UpdatePost : Gtk.ScrolledWindow
{
    [Gtk.Connect] private readonly Gtk.Label _titleLabel;
    [Gtk.Connect] private readonly Gtk.Label _subtitleLabel;
    [Gtk.Connect] private readonly Gtk.Label _contentsLabel;
    [Gtk.Connect] private readonly Gtk.Label _dateLabel;
    [Gtk.Connect] private readonly Gtk.Label _urlLabel;
    private UpdatePost(Gtk.Builder builder, string name) : base(new Gtk.Internal.ScrolledWindowHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
    }
    public UpdatePost(string contents, string date, string title, string feedname, string url, bool useMarkup) : this(new Gtk.Builder("UpdatePost.ui"), "_root")
    {
        _titleLabel.SetText(title);
        _subtitleLabel.SetText(feedname);
        if (useMarkup)
        {
            contents = FormatTextForMarkup(contents);
        }
        _contentsLabel.SetText(contents);
        if (useMarkup)
        {
            _contentsLabel.SetMarkup(contents);
        }
        _dateLabel.SetText(date);
        _urlLabel.SetMarkup("<a href='" + url + "'>Web link</a>");
        _urlLabel.SetTooltipText(url);

    }
    private string FormatTextForMarkup(string textToFormat)
    {
        textToFormat = textToFormat.Replace("[h3]", "<span font_weight=\"bold\" size=\"x-large\">").Replace("[/h3]", "</span>");
        textToFormat = textToFormat.Replace("[b]", "<b>").Replace("[/b]", "</b>");
        textToFormat = textToFormat.Replace("[i]", "<i>").Replace("[/i]", "</i>");
        textToFormat = textToFormat.Replace("[*]", " • ");
        textToFormat = textToFormat.Replace("[h2]", "<span font_weight=\"bold\" size=\"large\">").Replace("[/h2]", "</span>");
        textToFormat = System.Text.RegularExpressions.Regex.Replace(textToFormat, @"\[(.*?)\]", "");
        return textToFormat;
    }

}
