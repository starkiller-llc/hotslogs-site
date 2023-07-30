using System;

namespace HotsAdminConsole;

internal class TeamObjectiveRTO
{
    public bool IsWinner { get; set; }
    public int TeamObjectiveType { get; set; }
    public TimeSpan TimeSpan { get; set; }
    public int? CharacterID { get; set; }
    public int Value { get; set; }
}
