using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TeamProfilePlayer
{
    public int PID { get; set; }
    public string PN { get; set; }
    public int? BT { get; set; }
    public bool? IsLOO { get; set; }
}