using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideCharacterGameTimeWinRate
{
    public int GameTimeMinuteBegin { get; set; }
    public int GamesPlayed { get; set; }
    public decimal WinPercent { get; set; }
}