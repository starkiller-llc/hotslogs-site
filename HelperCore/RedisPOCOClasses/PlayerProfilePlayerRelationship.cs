using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class PlayerProfilePlayerRelationship
{
    public int PlayerID { get; set; }
    public string HeroPortraitURL { get; set; }
    public string PlayerName { get; set; }
    public string FavoriteHero { get; set; }
    public int GamesPlayedWith { get; set; }
    public decimal WinPercent { get; set; }
    public int CurrentMMR { get; set; }
}