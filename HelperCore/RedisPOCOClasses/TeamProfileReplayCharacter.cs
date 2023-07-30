using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TeamProfileReplayCharacter
{
    public int RID { get; set; }
    public int PID { get; set; }
    public bool IsAS { get; set; }
    public string C { get; set; }
    public int CL { get; set; }
    public bool IsW { get; set; }
    public int? MMRB { get; set; }
    public int? MMRC { get; set; }

    public TeamProfileReplayCharacterScoreResult SR { get; set; }
}