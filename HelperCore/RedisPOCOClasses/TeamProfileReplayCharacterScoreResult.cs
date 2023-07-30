using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class TeamProfileReplayCharacterScoreResult
{
    public int T { get; set; } = 0;
    public int S { get; set; } = 0;
    public int A { get; set; } = 0;
    public int D { get; set; } = 0;

    public int HD { get; set; } = 0;
    public int SiD { get; set; } = 0;
    public int StD { get; set; } = 0;
    public int MD { get; set; } = 0;
    public int CD { get; set; } = 0;
    public int SuD { get; set; } = 0;

    public TimeSpan? TCCdEH { get; set; } = null;

    public int? H { get; set; } = null;
    public int SH { get; set; } = 0;

    public int? DT { get; set; } = null;

    public int EC { get; set; } = 0;
    public int TK { get; set; } = 0;

    public TimeSpan TSD { get; set; } = TimeSpan.Zero;

    public int MCC { get; set; } = 0;
    public int WTC { get; set; } = 0;

    public int ME { get; set; } = 0; // Exp added to the player's Account and Hero level after the match
}