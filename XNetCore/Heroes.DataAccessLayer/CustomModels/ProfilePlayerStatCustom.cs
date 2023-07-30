using System;

namespace Heroes.DataAccessLayer.CustomModels;

public class ProfilePlayerStatCustom
{
    public int CharacterID { get; set; }
    public UInt64 GamesPlayed { get; set; }
    public decimal GamesWon { get; set; }
}
