using System;
using System.Collections.Generic;

namespace TalentsCli;

public class TalentUpdatePackage
{
    public int Build { get; set; }
    public string Version { get; set; }
    public DateTime Date { get; set; }
    public List<TalentInfo> TalentInfoList { get; set; }
}
