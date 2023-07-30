using System.Collections.Generic;

namespace HotsLogsApi.BL.Migration.Models;

public static class HeroRole
{
    public static readonly Dictionary<string, int> HeroRoleOrderDictionary = new()
    {
        { "Tank", 0 },
        { "Bruiser", 1 },
        { "Healer", 2 },
        { "Support", 3 },
        { "Ranged Assassin", 9 },
        { "Melee Assassin", 10 },
    };
}
