using System;
using System.Collections.Generic;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TeamProfileReplay
{
    public int RID { get; set; }
    public int GM { get; set; }
    public string M { get; set; }
    public TimeSpan RL { get; set; }
    public DateTime TR { get; set; }

    public List<TeamProfileReplayPeriodicXPBreakdown> PXPB { get; set; } = new();

    public List<TeamProfileReplayCharacter> RCs { get; set; } = new();
}