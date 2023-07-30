using System;

namespace Heroes.DataAccessLayer.CustomModels;

public class ProfileCharacterStatCustom
{
    public int CharacterID { get; set; }
    public int CharacterLevel { get; set; }
    public UInt64 GamesPlayed { get; set; }
    public TimeSpan AverageLength { get; set; }
    public decimal WinPercent { get; set; }
}
