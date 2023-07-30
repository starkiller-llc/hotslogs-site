using System.Collections.Generic;

namespace HotsLogsApi.BL.Migration.Helpers;

public static class TeamCompHelper
{
    public static readonly Dictionary<string, string> HeroRoleColorsDictionary = new()
    {
        { "Ranged Assassin", "#ef476f" }, // New roles
        { "Melee Assassin", "#f78c6b" }, // New roles
        { "Tank", "#ffd166" }, // New roles
        { "Bruiser", "#06d6a0" }, // New roles
        { "Healer", "#118ab2" }, // New roles
        { "Support", "#073b4c" }, // New roles
    };
}
