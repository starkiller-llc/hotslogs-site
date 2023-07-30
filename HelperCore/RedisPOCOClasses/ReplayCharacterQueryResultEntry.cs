namespace HelperCore.RedisPOCOClasses;

public class ReplayCharacterQueryResultEntry
{
    public string HeroPortraitURL { get; set; }
    public string Character { get; set; }
    public int GamesPlayed { get; set; }
    public decimal GamesPlayedPercent { get; set; }
    public decimal WinPercent { get; set; }
}