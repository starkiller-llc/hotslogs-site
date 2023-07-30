using Heroes.ReplayParser;
using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class PlayerMatch
{
    public int RID { get; set; }
    public GameMode GM { get; set; }
    public string M { get; set; }
    public TimeSpan RL { get; set; }
    public string C { get; set; }
    public int CL { get; set; }
    public bool IsW { get; set; }
    public int? MMRB { get; set; }
    public int? MMRC { get; set; }
    public bool IsRS { get; set; }
    public DateTime TR { get; set; }
}