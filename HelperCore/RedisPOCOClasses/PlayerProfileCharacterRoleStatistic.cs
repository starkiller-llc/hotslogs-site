using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class PlayerProfileCharacterRoleStatistic
{
    public string Role { get; set; }
    public int GamesPlayed { get; set; }
    public decimal WinPercent { get; set; }
}