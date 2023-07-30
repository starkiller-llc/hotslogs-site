using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class PlayerProfileMapStatistic
{
    public string Map { get; set; }
    public int GamesPlayed { get; set; }
    public TimeSpan AverageLength { get; set; }
    public decimal WinPercent { get; set; }
    public PlayerProfileCharacterStatistic[] MapDetailStatistics { get; set; }
}