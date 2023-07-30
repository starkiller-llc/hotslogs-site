using System;

namespace Heroes.DataAccessLayer.CustomModels;

public class ProfileGameTimeWinRateCustom
{
    public int ReplayLengthMinute { get; set; }
    public UInt64 GamesPlayed { get; set; }
    public decimal GamesWon { get; set; }
}
