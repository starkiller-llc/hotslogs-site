using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class PlayerProfileMMRMilestoneV3
{
    public int GameMode { get; set; }
    public DateTime MilestoneDate { get; set; }
    public int MMRRating { get; set; }
}