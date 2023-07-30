using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class PlayerProfileCharacterStatistic
{
    public string HeroPortraitURL { get; set; }
    public string Character { get; set; }
    public int? CharacterLevel { get; set; }
    public int GamesPlayed { get; set; }
    public TimeSpan AverageLength { get; set; }
    public decimal WinPercent { get; set; }
}