using System.Xml.Linq;

namespace CounterStats;

public class SteamProfile
{
    public string Avatar { get; }
    public string Name { get; }
    public string SecondaryName { get; }
    public string Location { get; }
    public string TimeCreated { get; }
    public string VacBanned { get; }
    public string HoursPlayed { get; }
    public string Summary { get; }

    public SteamProfile(string data)
    {
        var doc = XDocument.Parse(data);
        foreach (var a in doc.Descendants())
        {
            if (a.Name == "avatarFull" && a.Parent.Name == "profile")
            {
                Avatar = a.Value.ToString();
            }
            else if (a.Name == "steamID")
            {
                Name = a.Value.ToString();
            }
            else if (a.Name == "realname")
            {
                SecondaryName = a.Value.ToString();
            }
            else if (a.Name == "location")
            {
                Location = a.Value.ToString();
            }
            else if (a.Name == "memberSince")
            {
                TimeCreated = a.Value.ToString();
            }
            else if (a.Name == "vacBanned")
            {
                VacBanned = a.Value.ToString();
            }
            else if (a.Name == "hoursPlayed2Wk")
            {
                HoursPlayed = a.Value.ToString();
            }
            else if (a.Name == "summary" && a.Parent.Name == "profile")
            {
                Summary = a.Value.ToString();
            }
        }
    }
}
