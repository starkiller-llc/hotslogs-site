using System;

namespace Heroes.DataAccessLayer.CustomModels;

public class PlayerRelationshipCustom
{
    public int PlayerID { get; set; }
    public int FavoriteCharacter { get; set; }
    public string PlayerName { get; set; }
    public UInt64 GamesPlayedWith { get; set; }
    public decimal WinPercent { get; set; }
    public int CurrentMMR { get; set; }
}
