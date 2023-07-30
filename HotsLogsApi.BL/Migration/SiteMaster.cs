using HelperCore;
using Heroes.ReplayParser;
using HotsLogsApi.BL.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace HotsLogsApi.BL.Migration;

public static class SiteMaster
{
    public const string MasterAdvertisementBannerMonkeyBrokerTopScript = @"";

    private static readonly List<string> UsedLocalizedKeys = new()
    {
        "ChooseBattleNetId",
        "DefaultHeader",
        "DefaultIntroMessage",
        "ErrorPageTitle",
        "FAQMessageLine",
        "FAQTitle",
        "GenericAllLeagues",
        "GenericAverageLength",
        "GenericDateTime",
        "GenericEmailAddress",
        "GenericError",
        "GenericGameLength",
        "GenericGamesPlayed",
        "GenericGamesPlayedAgainst",
        "GenericGetPremium",
        "GenericHero",
        "GenericLastUpdatedMinutesAgo",
        "GenericLeaderboard",
        "GenericLeague",
        "GenericLogIn",
        "GenericLogOff",
        "GenericMapName",
        "GenericMapStatistics",
        "GenericMatchHistory",
        "GenericMMRBefore",
        "GenericMMRChange",
        "GenericMMRMilestones",
        "GenericOpposingHero",
        "GenericPassword",
        "GenericPlayer",
        "GenericPlayerSearchButton",
        "GenericPlayerSearchHelperText",
        "GenericProfile",
        "GenericRating",
        "GenericRegion",
        "GenericResult",
        "GenericSubmitButton",
        "GenericTeam",
        "GenericTimeSpan",
        "GenericTournaments",
        "GenericUserName",
        "GenericViewMatchHistory",
        "GenericWeekOfDate",
        "GenericWinPercent",
        "HeroDetailsTitle",
        "LeaderboardCurrentPlayerRank",
        "LeaderboardLeagueGamesPlayedRequirement",
        "LeaderboardMMRInfoLink",
        "LogInForgotPassword",
        "LogInNewUser",
        "ManageAccount",
        "MatchHistory",
        "MMRInfoMessageLine",
        "MMRInfoTitle",
        "PlayerSearchNoPlayersFound",
        "PlayerSearchTitle",
        "PrivacyPageTitle",
        "ProfileCurrentMMR",
        "ProfileTotalGamesPlayed",
        "ProfileTotalTimePlayed",
        "RegisterConfirmPassword",
        "RegisterMessageLine",
        "RegisterTitle",
        "ResetPasswordTitle",
        "SitewideHeroAndMapStatisticsTitle",
        "SitewideHeroStatistics",
        "SitewidePremiumTitle",
        "SitewideStatisticsTitle",
        "SitewideTalentsTitle",
        "SitewideUploadReplaysTitle",
        "TOSPageTitle",
        "UploadMessage",
        "UploadTitle",
    };

    public static Dictionary<string, string> GetAllStrings()
    {
        var r = LocalizedText.ResourceManager.GetResourceSet(
            culture: Thread.CurrentThread.CurrentCulture,
            createIfNotExists: true, // Needed to actually populate the resource set
            tryParents: true); // Return 'en' if 'en-US' requested for example...

        var rc = new Dictionary<string, string>();
        if (r is null)
        {
            return rc;
        }

        foreach (DictionaryEntry e in r)
        {
            if (e.Key is not string key || e.Value is not string value || !UsedLocalizedKeys.Any(key.StartsWith))
            {
                continue;
            }

            rc[key] = value;
        }

        return rc;
    }

    public static string GetGaugeHtml(
        decimal? value,
        decimal? min = 0.0m,
        decimal? max = 1.0m,
        string color = "#3BE33B",
        string formatString = "p1")
    {
        if (!value.HasValue)
        {
            return null;
        }

        var valueN = value.Value.ToString(CultureInfo.InvariantCulture);

        var valueDisplayString = value.Value.ToString(formatString);

        if (!min.HasValue || !max.HasValue || min.Value == max.Value)
        {
            return $"{valueDisplayString}<!-- $val:{valueN}$ -->";
        }

        if (value.Value < min.Value)
        {
            value = min.Value;
        }
        else if (value.Value > max.Value)
        {
            value = max.Value;
        }

        value = (value.Value - min.Value) / (max.Value - min.Value);
        valueN = value.Value.ToString(CultureInfo.InvariantCulture);

        return
            $@"{valueDisplayString}<!-- $val:{valueN}$ --><div style='height: 4px; width: {(value.Value * 100).ToString(CultureInfo.InvariantCulture)}%; background-color: {color};'></div>";
    }

    public static string GetLocalizedString(string keyPrefix, string value)
    {
        return LocalizedText.ResourceManager.GetString(keyPrefix + value.PrepareForImageURL()) ?? value;
    }

    public static string GetUserFriendlyTimeSpanString(TimeSpan timeSpan)
    {
        var dayOrDays = timeSpan.Days == 1
            ? LocalizedText.GenericTimeSpanDay
            : LocalizedText.GenericTimeSpanDayPlural;
        var hourOrHours = timeSpan.Hours == 1
            ? LocalizedText.GenericTimeSpanHour
            : LocalizedText.GenericTimeSpanHourPlural;
        var minuteOrMinutes = timeSpan.Minutes == 1
            ? LocalizedText.GenericTimeSpanMinute
            : LocalizedText.GenericTimeSpanMinutePlural;
        return (timeSpan.Days > 0 ? $"{string.Format(dayOrDays, timeSpan.Days)}, " : string.Empty) +
               (timeSpan.Hours > 0 ? $"{string.Format(hourOrHours, timeSpan.Hours)}, " : string.Empty) +
               string.Format(minuteOrMinutes, timeSpan.Minutes);
    }

    public static GameMode UserDefaultGameMode(string cookie)
    {
        return int.TryParse(cookie, out var selectedGameMode) &&
               DataHelper.GameModeWithStatistics.Any(i => i == selectedGameMode)
            ? (GameMode)selectedGameMode
            : GameMode.StormLeague;
    }
}
