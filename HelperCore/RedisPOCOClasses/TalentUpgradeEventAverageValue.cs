using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TalentUpgradeEventAverageValue
{
    public int UpgradeEventType { get; set; }
    public decimal AverageUpgradeEventValue { get; set; }
    public int GamesPlayed { get; set; }
    public decimal WinPercent { get; set; }
}