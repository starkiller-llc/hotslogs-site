using System;

namespace Heroes.DataAccessLayer.CustomModels;

public class ProfileMapStatCustom
{
    public int MapID { get; set; }
    public UInt64 GamesPlayed { get; set; }
    public TimeSpan AverageLength { get; set; }
    public decimal WinPercent { get; set; }
}
