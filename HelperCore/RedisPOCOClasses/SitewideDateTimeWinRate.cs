using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideDateTimeWinRate
{
    public DateTime DateTimeBegin { get; set; }
    public DateTime DateTimeEnd { get; set; }
    public int GamesPlayed { get; set; }
    public decimal WinPercent { get; set; }
}