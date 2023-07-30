using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class RankedSeason
{
    public string Title { get; set; }
    public DateTime DateTimeSeasonStart { get; set; }
    public DateTime DateTimeSeasonEnd { get; set; }
}
