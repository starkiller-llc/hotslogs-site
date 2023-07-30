using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class PlayerMatchHistory
{
    public PlayerMatch[] PlayerMatches { get; set; }
    public int PlayerID { get; set; }
    public string PlayerName { get; set; }
    public int[] OtherPlayerIDs { get; set; } = null;
    public string[] OtherPlayerNames { get; set; } = null;
    public DateTime LastUpdated { get; set; }
}