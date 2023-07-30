using Heroes.DataAccessLayer.Models;

namespace TalentsCli;

public class DiffEntry
{
    public HeroTalentInformation Talent { get; set; }
    public HeroTalentInformation NextTalent { get; set; }
    public int? NextRange { get; set; }
    public bool IsNew { get; set; }
    public bool IsRemovedInNext { get; set; }
    public bool IsTierChanged { get; set; }
    public bool IsDescriptionChanged { get; set; }
}
