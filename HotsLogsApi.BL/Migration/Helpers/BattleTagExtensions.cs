using System;
using System.Text.RegularExpressions;

namespace HotsLogsApi.BL.Migration.Helpers;

public static class BattleTagExtensions
{
    public static (string bTagName, int bTagNumber) ParseBattleTag(this string s)
    {
        var m = Regex.Match(s, @"^(?<nickname>[^#]+)#(?<battleTag>\d+)$");
        if (!m.Success)
        {
            throw new FormatException("Not a well formed battle tag");
        }

        var nickname = m.Groups["nickname"].ToString();
        var battleTag = int.Parse(m.Groups["battleTag"].ToString());
        return (nickname, battleTag);
    }

    public static bool TryParseBattleTag(this string s, out string nickname, out int battleTag)
    {
        var m = Regex.Match(s, @"^(?<nickname>[^#]+)#(?<battleTag>\d+)$");
        if (!m.Success)
        {
            nickname = default;
            battleTag = default;
            return false;
        }

        nickname = m.Groups["nickname"].ToString();
        battleTag = int.Parse(m.Groups["battleTag"].ToString());
        return true;
    }
}
