using Heroes.DataAccessLayer.CustomModels;
using Heroes.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HotsLogsApi.BL.Migration.Helpers;

public class ReplayCharacterHelper
{
    private readonly HeroesdataContext _dc;

    public ReplayCharacterHelper(HeroesdataContext dc)
    {
        _dc = dc;
    }

    public ReplayCharacterDetails[] GetReplayCharacterDetails(int replayID)
    {
        const string characterDetailsSql =
            @"select
                r.ReplayID,
                rc.PlayerID,
                p.BattleNetRegionId,
                p.BattleNetSubId,
                p.BattleNetId,
                p.`Name`,
                rc.CharacterID,
                rc.CharacterLevel,
                rc.IsWinner,
                rc.MMRBefore,
                rc.MMRChange,
                loo.PlayerID is not null as IsLeaderboardOptOut,

                rcsr.`Level`,
                rcsr.Takedowns,
                rcsr.SoloKills,
                rcsr.Assists,
                rcsr.Deaths,
                rcsr.HighestKillStreak,
                rcsr.HeroDamage,
                rcsr.SiegeDamage,
                rcsr.StructureDamage,
                rcsr.MinionDamage,
                rcsr.CreepDamage,
                rcsr.SummonDamage,
                rcsr.TimeCCdEnemyHeroes,
                rcsr.Healing,
                rcsr.SelfHealing,
                rcsr.DamageTaken,
                rcsr.ExperienceContribution,
                rcsr.TownKills,
                rcsr.TimeSpentDead,
                rcsr.MercCampCaptures,
                rcsr.WatchTowerCaptures,
                rcsr.MetaExperience,

                group_concat(rcma.MatchAwardType order by rcma.MatchAwardType) as MatchAwardTypes,

                coalesce(rep.reputation,0) Reputation

                from Replay r
                join ReplayCharacter rc on rc.ReplayID = r.ReplayID
                left join reputation rep on rep.PlayerId = rc.PlayerID
                left join ReplayCharacterScoreResult rcsr on rcsr.ReplayID = rc.ReplayID and rcsr.PlayerID = rc.PlayerID
                left join ReplayCharacterMatchAward rcma on rcma.ReplayID = rc.ReplayID and rcma.PlayerID = rc.PlayerID
                join Player p on p.PlayerID = rc.PlayerID
                left join LeaderboardOptOut loo on loo.PlayerID = p.PlayerID
                where r.ReplayID = {0}
                group by r.ReplayID, rc.PlayerID";

        const string talentsSql =
            @"select rc.PlayerID, h.TalentTier, coalesce(h.TalentName, concat('Talent ', t.TalentID)) as TalentName, h.TalentDescription
                from ReplayCharacter rc
                join ReplayCharacterTalent t on t.ReplayID = rc.ReplayID and t.PlayerID = rc.PlayerID
                join Replay r on r.ReplayID = rc.ReplayID
                join LocalizationAlias la on la.IdentifierID = rc.CharacterID
                join HeroTalentInformation h on h.`Character` = la.PrimaryName and r.ReplayBuild >= h.ReplayBuildFirst and r.ReplayBuild <= h.ReplayBuildLast and h.TalentID = t.TalentID
                where rc.ReplayID = {0}
                order by rc.PlayerID";

        var locDic = Global.GetLocalizationAliasesPrimaryNameDictionary();

        var q = _dc.ReplayCharacterDetailsEnumerable
            .FromSqlRaw(characterDetailsSql, replayID)
            .Include(r => r.ReplayCharacterScoreResult);
        var replayCharacterDetailsDictionary = q.ToDictionary(r => r.PlayerID);
        foreach (var entry in replayCharacterDetailsDictionary)
        {
            entry.Value.Character = locDic.ContainsKey(entry.Value.CharacterID)
                ? locDic[entry.Value.CharacterID]
                : "Unknown";
        }

        var q2 = _dc.ReplayCharacterTalentsCustom.FromSqlRaw(talentsSql, replayID);
        foreach (var e in q2)
        {
            if (!replayCharacterDetailsDictionary.ContainsKey(e.PlayerID))
            {
                continue;
            }

            var replayCharacterDetail =
                replayCharacterDetailsDictionary[e.PlayerID];
            var talentName = e.TalentName;
            var talentImageURL = Global.HeroTalentImages[replayCharacterDetail.Character, talentName];
            var talentNameDecription = talentName + ": " + e.TalentDescription;

            switch (e.TalentTier)
            {
                case 1:
                    replayCharacterDetail.TalentImageURL01 = talentImageURL;
                    replayCharacterDetail.TalentNameDescription01 = talentNameDecription;
                    replayCharacterDetail.TalentName01 = talentName;
                    break;
                case 4:
                    replayCharacterDetail.TalentImageURL04 = talentImageURL;
                    replayCharacterDetail.TalentNameDescription04 = talentNameDecription;
                    replayCharacterDetail.TalentName04 = talentName;
                    break;
                case 7:
                    replayCharacterDetail.TalentImageURL07 = talentImageURL;
                    replayCharacterDetail.TalentNameDescription07 = talentNameDecription;
                    replayCharacterDetail.TalentName07 = talentName;
                    break;
                case 10:
                    replayCharacterDetail.TalentImageURL10 = talentImageURL;
                    replayCharacterDetail.TalentNameDescription10 = talentNameDecription;
                    replayCharacterDetail.TalentName10 = talentName;
                    break;
                case 13:
                    replayCharacterDetail.TalentImageURL13 = talentImageURL;
                    replayCharacterDetail.TalentNameDescription13 = talentNameDecription;
                    replayCharacterDetail.TalentName13 = talentName;
                    break;
                case 16:
                    replayCharacterDetail.TalentImageURL16 = talentImageURL;
                    replayCharacterDetail.TalentNameDescription16 = talentNameDecription;
                    replayCharacterDetail.TalentName16 = talentName;
                    break;
                case 20:
                    replayCharacterDetail.TalentImageURL20 = talentImageURL;
                    replayCharacterDetail.TalentNameDescription20 = talentNameDecription;
                    replayCharacterDetail.TalentName20 = talentName;
                    break;
            }
        }

        return replayCharacterDetailsDictionary.Values
            .OrderByDescending(i => i.IsWinner)
            .ThenBy(i => i.PlayerName)
            .ThenByDescending(i => i.MMRBefore).ToArray();
    }
}
