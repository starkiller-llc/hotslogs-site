using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.ReplayParser;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotsAdminConsole.Services;

[UsedImplicitly]
[HotsService("Group Finder Listing", Sort = 1, Port = 17012)]
public class RedisGroupFinderListing : ServiceBase
{
    private const string GroupFinderListingSelectQuery =
        @"select q.PlayerID, q.GroupFinderListingTypeID, q.Information, q.MMRSearchRadius, q.BattleNetRegionId, q.`Name`, q.BattleTag, outerPMM.MMRRating from
            (select g.PlayerID, g.GroupFinderListingTypeID, g.Information, g.MMRSearchRadius, p.BattleNetRegionId, p.`Name`, p.BattleTag, max(pmm.MilestoneDate) as MaxMilestoneDate
            from GroupFinderListing g
            join Player p on p.PlayerID = g.PlayerID
            join PlayerMMRMilestoneV3 pmm on pmm.PlayerID = g.PlayerID and pmm.GameMode = g.GroupFinderListingTypeID
            where p.BattleNetRegionId = @battleNetRegionId and g.GroupFinderListingTypeID = @groupFinderListingTypeID
            group by g.PlayerID) q
            join PlayerMMRMilestoneV3 outerPMM on outerPMM.PlayerID = q.PlayerID and outerPMM.GameMode = q.GroupFinderListingTypeID and outerPMM.MilestoneDate = q.MaxMilestoneDate
            order by q.`Name`, q.BattleTag";

    private const string GroupFinderListingDeleteExpiredEntriesQuery =
        @"delete from GroupFinderListing where TimestampExpiration < now()";

    public RedisGroupFinderListing(IServiceProvider svcp) : base(svcp) { }

    protected override Task RunOnce(CancellationToken token = default)
    {
        using var scope = Svcp.CreateScope();
        using var mySqlConnection = new MySqlConnection(ConnectionString);
        mySqlConnection.Open();

        // Delete expired listings
        using (var mySqlCommand = new MySqlCommand(GroupFinderListingDeleteExpiredEntriesQuery, mySqlConnection)
               {
                   CommandTimeout = MMR.LongCommandTimeout,
               })
        {
            mySqlCommand.ExecuteNonQuery();
        }

        var redisClient = MyDbWrapper.Create(scope);
        foreach (var battleNetRegionId in DataHelper.BattleNetRegionNames.Keys)
        {
            foreach (var groupFinderListingTypeId in DataHelper.GameModeWithMMR.Where(
                         i =>
                             i == (int)GameMode.QuickMatch || i == (int)GameMode.StormLeague))
            {
                // Query active listings
                var groupFinderListingList = new List<GroupFinderListingPOCO>();
                using (var mySqlCommand = new MySqlCommand(GroupFinderListingSelectQuery, mySqlConnection)
                       {
                           CommandTimeout = MMR.LongCommandTimeout,
                       })
                {
                    mySqlCommand.Parameters.AddWithValue("@battleNetRegionId", battleNetRegionId);
                    mySqlCommand.Parameters.AddWithValue("@groupFinderListingTypeID", groupFinderListingTypeId);

                    using var mySqlDataReader = mySqlCommand.ExecuteReader();
                    while (mySqlDataReader.Read())
                    {
                        groupFinderListingList.Add(
                            new GroupFinderListingPOCO
                            {
                                PlayerID = mySqlDataReader.GetInt32("PlayerID"),
                                Name = mySqlDataReader.GetString("Name"),
                                BattleTag = mySqlDataReader.GetInt32("BattleTag"),
                                Information = mySqlDataReader.GetString("Information"),
                                MMRRatingSearchRangeBegin =
                                    mySqlDataReader.GetInt32("MMRRating") -
                                    mySqlDataReader.GetInt32("MMRSearchRadius"),
                                MMRRatingSearchRangeEnd =
                                    mySqlDataReader.GetInt32("MMRRating") +
                                    mySqlDataReader.GetInt32("MMRSearchRadius"),
                                MMRRating = mySqlDataReader.GetInt32("MMRRating"),
                            });
                    }
                }

                redisClient.TrySet(
                    "HOTSLogs:GroupFinderListings:" + battleNetRegionId + ":" + groupFinderListingTypeId,
                    new GroupFinderListings
                    {
                        ReplaySharesList = groupFinderListingList.ToArray(),
                        BattleNetRegionId = battleNetRegionId,
                        GroupFinderListingTypeID = groupFinderListingTypeId,
                        LastUpdated = DateTime.UtcNow,
                    },
                    TimeSpan.FromDays(3));
            }
        }

        return Task.CompletedTask;
    }
}
