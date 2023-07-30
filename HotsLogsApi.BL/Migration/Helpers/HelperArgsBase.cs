using System.Collections.Generic;

namespace HotsLogsApi.BL.Migration.Helpers;

public class HelperArgsBase
{
    public string GameMode { get; set; }
    public string GameModeEx { get; set; }
    public string Tournament { get; set; }
    public string Hero { get; set; }
    public List<int> League { get; set; }
    public List<string> Map { get; set; }
    public List<string> Patch { get; set; }
    public List<string> Time { get; set; }
    public List<string> Level { get; set; }
    public List<int> GameLength { get; set; }
}
