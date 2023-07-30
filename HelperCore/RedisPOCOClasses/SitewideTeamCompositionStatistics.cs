using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideTeamCompositionStatistics
{
    public SitewideTeamCompositionStatistic[] SitewideTeamCompositionStatisticArray { get; set; }
    public DateTime DateTimeBegin { get; set; }
    public DateTime DateTimeEnd { get; set; }
    public string Character { get; set; }
    public string MapName { get; set; }
    public DateTime LastUpdated { get; set; }
}