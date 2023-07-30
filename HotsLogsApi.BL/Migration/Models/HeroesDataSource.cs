namespace HotsLogsApi.BL.Migration.Models;

public class HeroesDataSource
{
    public HeroesDataSource(string displayName, string primaryName)
    {
        DisplayName = displayName;
        PrimaryName = primaryName;
    }

    public string DisplayName { get; }
    public string PrimaryName { get; }
}
