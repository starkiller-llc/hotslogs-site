using Heroes.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace Heroes.DataAccessLayer.Test;

internal class Program
{
    private static void Main(string[] args)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<Program>();
        var config = configBuilder.Build();
        var connectionString = config.GetConnectionString("DefaultConnection");
        var builder = new DbContextOptionsBuilder<HeroesdataContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        var options = builder.Options;

        using var dc = new HeroesdataContext(options);

        var replaysEx = dc.Replays
            .Include(x => x.ReplayCharacters)
            .ThenInclude(x => x.ReplayCharacterTalents)
            .Include(x => x.ReplayCharacters)
            .ThenInclude(x => x.Player)
            .Take(10).ToList();

        var mmrrecalcs = dc.MmrRecalcs.ToList();
        var logerrors = dc.LogErrors.Take(10).ToList();
        var mountinformations = dc.MountInformations.Take(10).ToList();
        var players = dc.Players.Take(10).ToList();
        var playeraggregates = dc.PlayerAggregates.Take(10).ToList();
        var playeralts = dc.PlayerAlts.Take(10).ToList();
        var playerbanneds = dc.PlayerBanneds.Take(10).ToList();
        var playerbannedleaderboards = dc.PlayerBannedLeaderboards.Take(10).ToList();
        var playerdisablenamechanges = dc.PlayerDisableNameChanges.Take(10).ToList();
        var playermmrmilestonev3s = dc.PlayerMmrMilestoneV3s.Take(10).ToList();
        var playermmrresets = dc.PlayerMmrResets.Take(10).ToList();
        var premiumpayments = dc.PremiumPayments.Take(10).ToList();
        var replays = dc.Replays.Take(10).ToList();
        var replayDups = dc.ReplayDups.Take(10).ToList();
        var replaycharacters = dc.ReplayCharacters.Take(10).ToList();
        var replaycharactermatchawards = dc.ReplayCharacterMatchAwards.Take(10).ToList();
        var replaycharacterscoreresults = dc.ReplayCharacterScoreResults.Take(10).ToList();
        var replaycharactersilenceds = dc.ReplayCharacterSilenceds.Take(10).ToList();
        var replaycharactertalents = dc.ReplayCharacterTalents.Take(10).ToList();
        var replaycharacterupgradeeventreplaylengthpercents =
            dc.ReplayCharacterUpgradeEventReplayLengthPercents.Take(10).ToList();
        var replayperiodicxpbreakdowns = dc.ReplayPeriodicXpBreakdowns.Take(10).ToList();
        var replayshares = dc.ReplayShares.Take(10).ToList();
        var replayteamherobans = dc.ReplayTeamHeroBans.Take(10).ToList();
        var replayteamobjectives = dc.ReplayTeamObjectives.Take(10).ToList();
        var unknowndata = dc.UnknownData.Take(10).ToList();
        var zamusers = dc.ZamUsers.Take(10).ToList();

        var amazonreplacementbuckets = dc.AmazonReplacementBuckets.Take(10).ToList();
        var buildnumbers = dc.BuildNumbers.Take(10).ToList();
        var dataupdates = dc.DataUpdates.Take(10).ToList();
        var events = dc.Events.Take(10).ToList();
        var groupfinderlistings = dc.GroupFinderListings.Take(10).ToList();
        var heroiconinformations = dc.HeroIconInformations.Take(10).ToList();
        var herotalentinformations = dc.HeroTalentInformations.Take(10).ToList();
        var net48Users = dc.Net48Users.Take(10).ToList();
        var hotsapireplays = dc.HotsApiReplays.Take(10).ToList();
        var hotsapitalents = dc.HotsApiTalents.Take(10).ToList();
        var leaderboardoptouts = dc.LeaderboardOptOuts.Take(10).ToList();
        var leaderboardrankings = dc.LeaderboardRankings.Take(10).ToList();
        var leagues = dc.Leagues.Take(10).ToList();
        var localizationaliases = dc.LocalizationAliases.Take(10).ToList();
    }
}
