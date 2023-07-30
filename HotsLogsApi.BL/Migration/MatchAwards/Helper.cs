using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.MatchAwards.Models;
using HotsLogsApi.BL.Resources;
using HotsLogsApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace HotsLogsApi.BL.Migration.MatchAwards;

public class Helper : HelperBase<MatchAwardsResponse>
{
    private readonly AppUser _appUser;
    private readonly MatchAwardsArgs _args;

    public Helper(MatchAwardsArgs args, AppUser appUser, IServiceProvider svcp) : base(svcp, args)
    {
        _args = args;
        _appUser = appUser;
    }

    public override MatchAwardsResponse MainCalculation()
    {
        using var scope = Svcp.CreateScope();
        var res = new MatchAwardsResponse();

        var teams = GetTeamsIfTournamentSelected(scope);
        res.Teams = teams;

        var selectedGameMode = GetSelectedGameMode();
        var selectedLeague = GetSelectedLeagues()[0];
        var selectedPlayerID = _args.PlayerId ?? 0;

        string title = null;
        var pMostRecentDaysVisible = true;

        MatchAwardContainer matchAwardContainer = null;

        var heroesEntity = HeroesdataContext.Create(scope);
        Player player = null;
        if (selectedPlayerID != 0)
        {
            player = heroesEntity.Players
                .Include(x => x.LeaderboardOptOut)
                .SingleOrDefault(i => i.PlayerId == selectedPlayerID);
        }

        if (player is null)
        {
            title = "Sitewide Match Awards";

            matchAwardContainer = DataHelper.RedisCacheGet<MatchAwardContainer>(
                "HOTSLogs:MatchAwardsV2:" + selectedLeague + ":" + selectedGameMode);
        }
        else
        {
            var heOptedOut = player.LeaderboardOptOut is not null;
            var weAreHim = _appUser?.Id == player.PlayerId;
            if (heOptedOut && !weAreHim)
            {
                res.Unauthorized = true;
                return res;
            }

            title = "Player Match Awards: " + player.Name;

            var daysOfStatisticsToQuery = (int)(DateTime.UtcNow - new DateTime(2016, 9, 15))
                .TotalDays;
            matchAwardContainer = DataHelper.GetMatchAwardContainer(
                selectedGameMode == -1 ? DataHelper.GameModeWithStatistics : new[] { selectedGameMode },
                playerId: selectedPlayerID,
                daysOfStatisticsToQuery: daysOfStatisticsToQuery /* Beginning of MVP Commendation System */);

            const int minGamesPlayedForPlayerHero = 5;
            matchAwardContainer.StatisticStandard = matchAwardContainer.StatisticStandard
                .Where(i => i.GamesPlayedTotal > minGamesPlayedForPlayerHero).ToArray();
            matchAwardContainer.StatisticMapObjectives = matchAwardContainer.StatisticMapObjectives
                .Where(i => i.GamesPlayedTotal > minGamesPlayedForPlayerHero).ToArray();

            pMostRecentDaysVisible = false;
        }

        if (matchAwardContainer == null || matchAwardContainer.StatisticStandard.Length == 0 ||
            matchAwardContainer.StatisticMapObjectives.Length == 0)
        {
            goto End;
        }

        res.LastUpdatedText = string.Format(
            LocalizedText.GenericLastUpdatedMinutesAgo,
            (int)(DateTime.UtcNow - matchAwardContainer.LastUpdated).TotalMinutes);

        // Set RadGrid data source
        var
            heroRoleConcurrentDictionary = Global.GetHeroRoleConcurrentDictionary();
        var heroDic = Global.GetHeroAliasCSVConcurrentDictionary();

        var isStandardStatistics = _args.Type == 0;

        MatchAwardsRow[] resultStats;
        if (isStandardStatistics)
        {
            // Gather Standard statistics
            var gamesPlayedTotalMin = matchAwardContainer.StatisticStandard.Min(i => i.GamesPlayedTotal);
            var gamesPlayedTotalMax = matchAwardContainer.StatisticStandard.Max(i => i.GamesPlayedTotal);

            var percentGamesPlayedWithAwardMin =
                matchAwardContainer.StatisticMapObjectives.Min(
                    i => (decimal)i.GamesPlayedWithAward / i.GamesPlayedTotal);
            var percentGamesPlayedWithAwardMax =
                matchAwardContainer.StatisticMapObjectives.Max(
                    i => (decimal)i.GamesPlayedWithAward / i.GamesPlayedTotal);

            var percentMvpMin = matchAwardContainer.StatisticStandard.Min(i => i.PercentMVP);
            var percentMvpMax = matchAwardContainer.StatisticStandard.Max(i => i.PercentMVP);

            var percentHighestKillStreakMin =
                matchAwardContainer.StatisticStandard.Min(i => i.PercentHighestKillStreak);
            var percentHighestKillStreakMax =
                matchAwardContainer.StatisticStandard.Max(i => i.PercentHighestKillStreak);

            var percentMostXpContributionMin =
                matchAwardContainer.StatisticStandard.Min(i => i.PercentMostXPContribution);
            var percentMostXpContributionMax =
                matchAwardContainer.StatisticStandard.Max(i => i.PercentMostXPContribution);

            var percentMostHeroDamageDoneMin =
                matchAwardContainer.StatisticStandard.Min(i => i.PercentMostHeroDamageDone);
            var percentMostHeroDamageDoneMax =
                matchAwardContainer.StatisticStandard.Max(i => i.PercentMostHeroDamageDone);

            var percentMostSiegeDamageDoneMin =
                matchAwardContainer.StatisticStandard.Min(i => i.PercentMostSiegeDamageDone);
            var percentMostSiegeDamageDoneMax =
                matchAwardContainer.StatisticStandard.Max(i => i.PercentMostSiegeDamageDone);

            var percentMostDamageTakenMin =
                matchAwardContainer.StatisticStandard.Min(i => i.PercentMostDamageTaken);
            var percentMostDamageTakenMax =
                matchAwardContainer.StatisticStandard.Max(i => i.PercentMostDamageTaken);

            var percentMostHealingMin = matchAwardContainer.StatisticStandard.Min(i => i.PercentMostHealing);
            var percentMostHealingMax = matchAwardContainer.StatisticStandard.Max(i => i.PercentMostHealing);

            var percentMostStunsMin = matchAwardContainer.StatisticStandard.Min(i => i.PercentMostStuns);
            var percentMostStunsMax = matchAwardContainer.StatisticStandard.Max(i => i.PercentMostStuns);

            var percentMostMercCampsCapturedMin =
                matchAwardContainer.StatisticStandard.Min(i => i.PercentMostMercCampsCaptured);
            var percentMostMercCampsCapturedMax =
                matchAwardContainer.StatisticStandard.Max(i => i.PercentMostMercCampsCaptured);

            var percentMapSpecificMin = matchAwardContainer.StatisticStandard.Min(i => i.PercentMapSpecific);
            var percentMapSpecificMax = matchAwardContainer.StatisticStandard.Max(i => i.PercentMapSpecific);

            var percentMostKillsMin = matchAwardContainer.StatisticStandard.Min(i => i.PercentMostKills);
            var percentMostKillsMax = matchAwardContainer.StatisticStandard.Max(i => i.PercentMostKills);

            var percentHatTrickMin = matchAwardContainer.StatisticStandard.Min(i => i.PercentHatTrick);
            var percentHatTrickMax = matchAwardContainer.StatisticStandard.Max(i => i.PercentHatTrick);

            var percentClutchHealerMin = matchAwardContainer.StatisticStandard.Min(i => i.PercentClutchHealer);
            var percentClutchHealerMax = matchAwardContainer.StatisticStandard.Max(i => i.PercentClutchHealer);

            var percentMostProtectionMin =
                matchAwardContainer.StatisticStandard.Min(i => i.PercentMostProtection);
            var percentMostProtectionMax =
                matchAwardContainer.StatisticStandard.Max(i => i.PercentMostProtection);

            var percentZeroDeathsMin = matchAwardContainer.StatisticStandard.Min(i => i.PercentZeroDeaths);
            var percentZeroDeathsMax = matchAwardContainer.StatisticStandard.Max(i => i.PercentZeroDeaths);

            var percentMostRootsMin = matchAwardContainer.StatisticStandard.Min(i => i.PercentMostRoots);
            var percentMostRootsMax = matchAwardContainer.StatisticStandard.Max(i => i.PercentMostRoots);

            resultStats = matchAwardContainer.StatisticStandard.Select(
                    i => new MatchAwardsRow
                    {
                        HeroPortraitURL = Global.HeroPortraitImages[i.Character],
                        Character = SiteMaster.GetLocalizedString("GenericHero", i.Character),
                        CharacterURL = i.Character,
                        GamesPlayedTotal = SiteMaster.GetGaugeHtml(
                            i.GamesPlayedTotal,
                            gamesPlayedTotalMin,
                            gamesPlayedTotalMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Support"],
                            formatString: "N0"),
                        GamesPlayedWithAward = SiteMaster.GetGaugeHtml(
                            (decimal)i.GamesPlayedWithAward / i.GamesPlayedTotal,
                            percentGamesPlayedWithAwardMin,
                            percentGamesPlayedWithAwardMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Support"]),
                        PercentMVP = SiteMaster.GetGaugeHtml(
                            i.PercentMVP,
                            percentMvpMin,
                            percentMvpMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Healer"]),
                        PercentHighestKillStreak = SiteMaster.GetGaugeHtml(
                            i.PercentHighestKillStreak,
                            percentHighestKillStreakMin,
                            percentHighestKillStreakMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostXPContribution = SiteMaster.GetGaugeHtml(
                            i.PercentMostXPContribution,
                            percentMostXpContributionMin,
                            percentMostXpContributionMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Support"]),
                        PercentMostHeroDamageDone = SiteMaster.GetGaugeHtml(
                            i.PercentMostHeroDamageDone,
                            percentMostHeroDamageDoneMin,
                            percentMostHeroDamageDoneMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostSiegeDamageDone = SiteMaster.GetGaugeHtml(
                            i.PercentMostSiegeDamageDone,
                            percentMostSiegeDamageDoneMin,
                            percentMostSiegeDamageDoneMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostDamageTaken = SiteMaster.GetGaugeHtml(
                            i.PercentMostDamageTaken,
                            percentMostDamageTakenMin,
                            percentMostDamageTakenMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Tank"]),
                        PercentMostHealing = SiteMaster.GetGaugeHtml(
                            i.PercentMostHealing,
                            percentMostHealingMin,
                            percentMostHealingMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Healer"]),
                        PercentMostStuns = SiteMaster.GetGaugeHtml(
                            i.PercentMostStuns,
                            percentMostStunsMin,
                            percentMostStunsMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Bruiser"]),
                        PercentMostMercCampsCaptured = SiteMaster.GetGaugeHtml(
                            i.PercentMostMercCampsCaptured,
                            percentMostMercCampsCapturedMin,
                            percentMostMercCampsCapturedMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMapSpecific = SiteMaster.GetGaugeHtml(
                            i.PercentMapSpecific,
                            percentMapSpecificMin,
                            percentMapSpecificMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Support"]),
                        PercentMostKills = SiteMaster.GetGaugeHtml(
                            i.PercentMostKills,
                            percentMostKillsMin,
                            percentMostKillsMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentHatTrick = SiteMaster.GetGaugeHtml(
                            i.PercentHatTrick,
                            percentHatTrickMin,
                            percentHatTrickMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentClutchHealer = SiteMaster.GetGaugeHtml(
                            i.PercentClutchHealer,
                            percentClutchHealerMin,
                            percentClutchHealerMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Healer"]),
                        PercentMostProtection = SiteMaster.GetGaugeHtml(
                            i.PercentMostProtection,
                            percentMostProtectionMin,
                            percentMostProtectionMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Healer"]),
                        PercentZeroDeaths = SiteMaster.GetGaugeHtml(
                            i.PercentZeroDeaths,
                            percentZeroDeathsMin,
                            percentZeroDeathsMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Support"]),
                        PercentMostRoots = SiteMaster.GetGaugeHtml(
                            i.PercentMostRoots,
                            percentMostRootsMin,
                            percentMostRootsMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Bruiser"]),
                        Role = heroRoleConcurrentDictionary.ContainsKey(i.Character)
                            ? heroRoleConcurrentDictionary[i.Character]
                            : null,
                        AliasCSV = heroDic.ContainsKey(i.Character)
                            ? heroDic[i.Character]
                            : null,
                        GameMode = _args.GameModeEx,
                        Event = _args.Tournament,
                    })
                .OrderByDescending(i => i.GamesPlayedWithAward).ToArray();
        }
        else
        {
            var stats = matchAwardContainer.StatisticMapObjectives;
            // Gather Map Objective statistics
            var gamesPlayedTotalMin = stats.Min(i => i.GamesPlayedTotal);
            var gamesPlayedTotalMax = stats.Max(i => i.GamesPlayedTotal);

            var percentGamesPlayedWithAwardMin = stats.Min(i => (decimal)i.GamesPlayedWithAward / i.GamesPlayedTotal);
            var percentGamesPlayedWithAwardMax = stats.Max(i => (decimal)i.GamesPlayedWithAward / i.GamesPlayedTotal);

            var percentMostImmortalDamageMin = stats.Min(i => i.PercentMostImmortalDamage);
            var percentMostImmortalDamageMax = stats.Max(i => i.PercentMostImmortalDamage);

            var percentMostCoinsPaidMin = stats.Min(i => i.PercentMostCoinsPaid);
            var percentMostCoinsPaidMax = stats.Max(i => i.PercentMostCoinsPaid);

            var percentMostSkullsCollectedMin = stats.Min(i => i.PercentMostSkullsCollected);
            var percentMostSkullsCollectedMax = stats.Max(i => i.PercentMostSkullsCollected);

            var percentMostCurseDamageDoneMin = stats.Min(i => i.PercentMostCurseDamageDone);
            var percentMostCurseDamageDoneMax = stats.Max(i => i.PercentMostCurseDamageDone);

            var percentMostDragonShrinesCapturedMin = stats.Min(i => i.PercentMostDragonShrinesCaptured);
            var percentMostDragonShrinesCapturedMax = stats.Max(i => i.PercentMostDragonShrinesCaptured);

            var percentMostDamageToPlantsMin = stats.Min(i => i.PercentMostDamageToPlants);
            var percentMostDamageToPlantsMax = stats.Max(i => i.PercentMostDamageToPlants);

            var percentMostDamageToMinionsMin = stats.Min(i => i.PercentMostDamageToMinions);
            var percentMostDamageToMinionsMax = stats.Max(i => i.PercentMostDamageToMinions);

            var percentMostTimeInTempleMin = stats.Min(i => i.PercentMostTimeInTemple);
            var percentMostTimeInTempleMax = stats.Max(i => i.PercentMostTimeInTemple);

            var percentMostGemsTurnedInMin = stats.Min(i => i.PercentMostGemsTurnedIn);
            var percentMostGemsTurnedInMax = stats.Max(i => i.PercentMostGemsTurnedIn);

            var percentMostAltarDamageMin = stats.Min(i => i.PercentMostAltarDamage);
            var percentMostAltarDamageMax = stats.Max(i => i.PercentMostAltarDamage);

            var percentMostDamageDoneToZergMin = stats.Min(i => i.PercentMostDamageDoneToZerg);
            var percentMostDamageDoneToZergMax = stats.Max(i => i.PercentMostDamageDoneToZerg);

            var percentMostNukeDamageDoneMin = stats.Min(i => i.PercentMostNukeDamageDone);
            var percentMostNukeDamageDoneMax = stats.Max(i => i.PercentMostNukeDamageDone);

            resultStats = stats.Select(
                    i => new MatchAwardsRow
                    {
                        HeroPortraitURL = Global.HeroPortraitImages[i.Character],
                        Character = SiteMaster.GetLocalizedString("GenericHero", i.Character),
                        CharacterURL = i.Character,
                        GamesPlayedTotal = SiteMaster.GetGaugeHtml(
                            i.GamesPlayedTotal,
                            gamesPlayedTotalMin,
                            gamesPlayedTotalMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Support"],
                            formatString: "N0"),
                        GamesPlayedWithAward = SiteMaster.GetGaugeHtml(
                            (decimal)i.GamesPlayedWithAward / i.GamesPlayedTotal,
                            percentGamesPlayedWithAwardMin,
                            percentGamesPlayedWithAwardMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Support"]),
                        PercentMostDragonShrinesCaptured = SiteMaster.GetGaugeHtml(
                            i.PercentMostDragonShrinesCaptured,
                            percentMostDragonShrinesCapturedMin,
                            percentMostDragonShrinesCapturedMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostCurseDamageDone = SiteMaster.GetGaugeHtml(
                            i.PercentMostCurseDamageDone,
                            percentMostCurseDamageDoneMin,
                            percentMostCurseDamageDoneMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostCoinsPaid = SiteMaster.GetGaugeHtml(
                            i.PercentMostCoinsPaid,
                            percentMostCoinsPaidMin,
                            percentMostCoinsPaidMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostSkullsCollected = SiteMaster.GetGaugeHtml(
                            i.PercentMostSkullsCollected,
                            percentMostSkullsCollectedMin,
                            percentMostSkullsCollectedMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostDamageToPlants = SiteMaster.GetGaugeHtml(
                            i.PercentMostDamageToPlants,
                            percentMostDamageToPlantsMin,
                            percentMostDamageToPlantsMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostTimeInTemple = SiteMaster.GetGaugeHtml(
                            i.PercentMostTimeInTemple,
                            percentMostTimeInTempleMin,
                            percentMostTimeInTempleMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostGemsTurnedIn = SiteMaster.GetGaugeHtml(
                            i.PercentMostGemsTurnedIn,
                            percentMostGemsTurnedInMin,
                            percentMostGemsTurnedInMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostImmortalDamage = SiteMaster.GetGaugeHtml(
                            i.PercentMostImmortalDamage,
                            percentMostImmortalDamageMin,
                            percentMostImmortalDamageMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostDamageToMinions = SiteMaster.GetGaugeHtml(
                            i.PercentMostDamageToMinions,
                            percentMostDamageToMinionsMin,
                            percentMostDamageToMinionsMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostAltarDamage = SiteMaster.GetGaugeHtml(
                            i.PercentMostAltarDamage,
                            percentMostAltarDamageMin,
                            percentMostAltarDamageMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostDamageDoneToZerg = SiteMaster.GetGaugeHtml(
                            i.PercentMostDamageDoneToZerg,
                            percentMostDamageDoneToZergMin,
                            percentMostDamageDoneToZergMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        PercentMostNukeDamageDone = SiteMaster.GetGaugeHtml(
                            i.PercentMostNukeDamageDone,
                            percentMostNukeDamageDoneMin,
                            percentMostNukeDamageDoneMax,
                            color: TeamCompHelper.HeroRoleColorsDictionary["Ranged Assassin"]),
                        Role = heroRoleConcurrentDictionary.ContainsKey(i.Character)
                            ? heroRoleConcurrentDictionary[i.Character]
                            : null,
                        AliasCSV = heroDic.ContainsKey(i.Character)
                            ? heroDic[i.Character]
                            : null,
                    })
                .OrderByDescending(i => i.GamesPlayedWithAward).ToArray();
        }

        res.Stats = resultStats;

        //MatchAwardsGrid.Columns.FindByDataField("PercentMVP").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentHighestKillStreak").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostXPContribution").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostHeroDamageDone").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostSiegeDamageDone").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostDamageTaken").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostHealing").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostStuns").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostMercCampsCaptured").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMapSpecific").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostKills").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentHatTrick").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentClutchHealer").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostProtection").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentZeroDeaths").Visible = isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostRoots").Visible = isStandardStatistics;

        //MatchAwardsGrid.Columns.FindByDataField("PercentMostDragonShrinesCaptured").Visible = !isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostCurseDamageDone").Visible = !isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostCoinsPaid").Visible = !isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostSkullsCollected").Visible = !isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostDamageToPlants").Visible = !isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostTimeInTemple").Visible = !isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostGemsTurnedIn").Visible = !isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostImmortalDamage").Visible = !isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostDamageToMinions").Visible = !isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostAltarDamage").Visible = !isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostDamageDoneToZerg").Visible = !isStandardStatistics;
        //MatchAwardsGrid.Columns.FindByDataField("PercentMostNukeDamageDone").Visible = !isStandardStatistics;
        End:

        res.Title = title;
        res.MostRecentDaysVisible = pMostRecentDaysVisible;

        return res;
    }
}
