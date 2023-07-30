using Amazon.S3;
using Amazon.SimpleEmail;
using HelperCore;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using HotsLogsApi.BL.Amazon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceStackReplacement;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace HotsLogsApi.BL.Migration;

public static class Global
{
    public const decimal TeamDraftHelperDaysOfReplayCharacterResultsToCache = 150.0m;
    private static IServiceProvider _svcp;

    private static readonly object LockHeroRole = new object();
    private static readonly object LockLocalizationAliases = new object();
    private static readonly object LockHeroAliasCsv = new object();
    private static readonly object LockHeroPortraitImageDictionary = new object();
    private static readonly object LockLeagues = new object();
    private static readonly object LockHeroTalentImageDictionary = new object();
    private static readonly object LockObjectLocalizationAliasesPrimaryNameDictionary = new object();
    private static readonly object LockObjectPlayerMMRResets = new object();
    private static readonly object LockObjectLocalizationAliasesIdentifierIdDictionary = new object();
    private static ConcurrentDictionary<string, string> _heroRoleDictionary;
    private static LocalizationAlias[] _localizationAliases;
    private static ConcurrentDictionary<string, string> _aliasDic;
    private static ConcurrentDictionary<string, string> _portraitImageDic;
    private static League[] _leagues;
    private static ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _talentImageDic;
    private static ConcurrentDictionary<int, string> _localizationAliasesPrimaryNameDictionary;
    private static PlayerMmrReset[] _playerMMRResets;
    private static ConcurrentDictionary<string, int> _localizationAliasesIdentifierIdDictionary;

    private static readonly IAmazonReplacementHandler AmazonReplacementHandler =
        new AmazonReplacementFileSystemHandler();

    static Global()
    {
        AmazonS3Client.GetObjectDelegate = AmazonReplacementHandler.GetObject;
        AmazonS3Client.PutObjectDelegate = AmazonReplacementHandler.PutObject;
        AmazonS3Client.DeleteObjectDelegate = AmazonReplacementHandler.DeleteObject;
        AmazonSimpleEmailServiceClient.SendEmailDelegate = AmazonReplacementHandler.SendEmail;
    }

    public static HeroPortraitImagesAccessor HeroPortraitImages { get; set; } = new HeroPortraitImagesAccessor();
    public static HeroTalentImagesAccessor HeroTalentImages { get; set; } = new HeroTalentImagesAccessor();

    public static DateTime RecentReplayCharacterResultsByMapAndHeroLastUpdated
    {
        get
        {
            using var scope = _svcp.CreateScope();
            var redisClient = MyDbWrapper.Create(scope);
            const string key = "HOTSLogs:TeamDraft:LastUpdated";
            return redisClient.ContainsKey(key)
                ? redisClient.Get<DateTime>(key)
                : DateTime.MinValue;
        }
    }

    // Recent Replay Character Results Cached and Grouped by GameMode and Map and two Heroes
    public static
        ConcurrentDictionary<int, ConcurrentDictionary<int,
            ConcurrentDictionary<sbyte, ConcurrentDictionary<sbyte, List<sbyte[][]>[]>>>>
        RecentReplayCharacterResultsByGameModeAndMapAndHero
    {
        get;
        set;
    }

    public static ConcurrentDictionary<string, string> GetHeroAliasCSVConcurrentDictionary()
    {
        lock (LockHeroAliasCsv)
        {
            if (_aliasDic == null)
            {
                _aliasDic = new ConcurrentDictionary<string, string>();
                foreach (var heroAliasCsvInformation in GetLocalizationAlias()
                             .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Hero))
                {
                    _aliasDic[heroAliasCsvInformation.PrimaryName] =
                        heroAliasCsvInformation.AliasesCsv;
                }
            }

            return _aliasDic;
        }
    }

    public static ConcurrentDictionary<string, string> GetHeroRoleConcurrentDictionary()
    {
        lock (LockHeroRole)
        {
            if (_heroRoleDictionary == null)
            {
                _heroRoleDictionary = new ConcurrentDictionary<string, string>();
                foreach (var heroRoleInformation in GetLocalizationAlias()
                             .Where(i => i.Type == (int)DataHelper.LocalizationAliasType.Hero))
                {
                    _heroRoleDictionary[heroRoleInformation.PrimaryName] = heroRoleInformation.NewGroup;
                }
            }

            return _heroRoleDictionary;
        }
    }

    public static League[] GetLeagues()
    {
        lock (LockLeagues)
        {
            if (_leagues == null)
            {
                if (!IsApplicationOfflineMode())
                {
                    using var scope = _svcp.CreateScope();
                    var heroesEntity = HeroesdataContext.Create(scope);
                    _leagues = heroesEntity.Leagues.ToArray();
                }
                else
                {
                    _leagues = new[] { new League { LeagueName = "Unknown" } };
                }
            }

            return _leagues;
        }
    }

    public static LocalizationAlias[] GetLocalizationAlias()
    {
        lock (LockLocalizationAliases)
        {
            if (_localizationAliases == null)
            {
                if (!IsApplicationOfflineMode())
                {
                    using var scope = _svcp.CreateScope();
                    var heroesEntity = HeroesdataContext.Create(scope);
                    _localizationAliases = heroesEntity.LocalizationAliases.Where(
                        i => i.Type == (int)DataHelper.LocalizationAliasType.Hero ||
                             i.Type == (int)DataHelper.LocalizationAliasType.Map).ToArray();
                }
                else
                {
                    _localizationAliases = new[]
                    {
                        new LocalizationAlias
                        {
                            PrimaryName = "Unknown",
                            Type = (int)DataHelper.LocalizationAliasType.Hero,
                        },
                    };
                }
            }
        }

        return _localizationAliases;
    }

    public static ConcurrentDictionary<string, int> GetLocalizationAliasesIdentifierIDDictionary()
    {
        lock (LockObjectLocalizationAliasesIdentifierIdDictionary)
        {
            if (_localizationAliasesIdentifierIdDictionary == null)
            {
                _localizationAliasesIdentifierIdDictionary = new ConcurrentDictionary<string, int>();
                foreach (var localizationAlias in GetLocalizationAlias())
                {
                    _localizationAliasesIdentifierIdDictionary[localizationAlias.PrimaryName] =
                        localizationAlias.IdentifierId;
                }
            }

            return _localizationAliasesIdentifierIdDictionary;
        }
    }

    public static ConcurrentDictionary<int, string> GetLocalizationAliasesPrimaryNameDictionary()
    {
        lock (LockObjectLocalizationAliasesPrimaryNameDictionary)
        {
            if (_localizationAliasesPrimaryNameDictionary == null)
            {
                _localizationAliasesPrimaryNameDictionary = new ConcurrentDictionary<int, string>();
                foreach (var localizationAlias in GetLocalizationAlias())
                {
                    _localizationAliasesPrimaryNameDictionary[localizationAlias.IdentifierId] =
                        localizationAlias.PrimaryName;
                }
            }

            return _localizationAliasesPrimaryNameDictionary;
        }
    }

    public static PlayerMmrReset[] GetPlayerMMRResets()
    {
        lock (LockObjectPlayerMMRResets)
        {
            if (_playerMMRResets == null)
            {
                if (!IsApplicationOfflineMode())
                {
                    using var scope = _svcp.CreateScope();
                    var heroesEntity = HeroesdataContext.Create(scope);
                    _playerMMRResets = heroesEntity.PlayerMmrResets.OrderBy(i => i.ResetDate).ToArray();
                }
                else
                {
                    _playerMMRResets = Array.Empty<PlayerMmrReset>();
                }
            }
        }

        return _playerMMRResets;
    }

    public static void SetServiceProvider(IServiceProvider svcp)
    {
        _svcp = svcp;
    }

    internal static bool IsEventGameMode(int gameMode)
    {
        // TODO: ffs do this properly!
        return gameMode > 100;
    }

    internal static bool IsEventGameMode(string gameMode)
    {
        if (!int.TryParse(gameMode, out var gameModeInt))
        {
            return false;
        }

        return IsEventGameMode(gameModeInt);
    }

    private static ConcurrentDictionary<string, string> GetHeroPortraitImageConcurrentDictionary()
    {
        lock (LockHeroPortraitImageDictionary)
        {
            if (_portraitImageDic == null)
            {
                _portraitImageDic = new ConcurrentDictionary<string, string>();

                var scopedSvcp = _svcp.CreateScope();
                var dc = scopedSvcp.ServiceProvider.GetRequiredService<HeroesdataContext>();

                foreach (var row in dc.HeroIconInformations)
                {
                    _portraitImageDic[row.Name] = row.Icon;
                }
            }

            return _portraitImageDic;
        }
    }

    private static ConcurrentDictionary<string, ConcurrentDictionary<string, string>>
        GetHeroTalentImageConcurrentDictionary()
    {
        lock (LockHeroTalentImageDictionary)
        {
            if (_talentImageDic == null)
            {
                _talentImageDic =
                    new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>(
                        StringComparer.OrdinalIgnoreCase);
                using var scope = _svcp.CreateScope();
                var heroesEntity = HeroesdataContext.Create(scope);
                foreach (var row in heroesEntity.TalentImageMappings)
                {
                    if (!_talentImageDic.ContainsKey(row.Character))
                    {
                        _talentImageDic[row.Character] =
                            new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }

                    _talentImageDic[row.Character][row.TalentName] = row.TalentImage;
                }
            }

            return _talentImageDic;
        }
    }

    private static bool IsApplicationOfflineMode()
    {
        // return DataHelper.RedisCacheGet<string>("HOTSLogs:OfflineMode") == "1" && false;
        return false;
    }


    public class HeroPortraitImagesAccessor
    {
        public string this[string heroName]
        {
            get
            {
                var heroPortraitImageConcurrentDictionary =
                    GetHeroPortraitImageConcurrentDictionary();

                if (heroName != null && heroPortraitImageConcurrentDictionary.ContainsKey(heroName))
                {
                    return heroPortraitImageConcurrentDictionary[heroName];
                }

                return "/Images/Heroes/Portraits/Unknown.png";
            }
        }
    }

    public class HeroTalentImagesAccessor
    {
        public string this[string heroName, string talentName]
        {
            get
            {
                if (heroName != null && talentName != null)
                {
                    var dic = GetHeroTalentImageConcurrentDictionary();

                    if (dic.ContainsKey(heroName) && dic[heroName].ContainsKey(talentName))
                    {
                        return dic[heroName][talentName];
                    }
                }

                return "/Images/Heroes/Portraits/Unknown.png";
            }
        }
    }
}
