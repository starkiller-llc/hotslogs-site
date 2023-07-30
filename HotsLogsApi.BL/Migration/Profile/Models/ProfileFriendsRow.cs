namespace HotsLogsApi.BL.Migration.Profile.Models;

public class ProfileFriendsRow
{
    public string HeroPortraitURL { get; set; }
    public int PlayerID { get; set; }
    public string PlayerName { get; set; }
    public string FavoriteHero { get; set; }
    public string FavoriteHeroURL { get; set; }
    public string GamesPlayedWith { get; set; }
    public string WinPercent { get; set; }
    public int CurrentMMR { get; set; }
}
