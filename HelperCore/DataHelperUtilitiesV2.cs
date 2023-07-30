using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace HelperCore;

public static partial class DataHelper
{
    public static List<GameEvent> GetGameEvents()
    {
        using var scope = _svcp.CreateScope();
        var heroesEntity = HeroesdataContext.Create(scope);
        var q = (from x in heroesEntity.Events
                .Include(x => x.EventIdparentNavigation)
                .AsEnumerable()
            select new GameEvent
            {
                Id = x.EventId,
                Name = x.EventName,
                ParentId = x.EventIdparent,
                ParentName = x.EventIdparentNavigation?.EventName,
            }).ToList();

        var ids = q.Select(x => x.Id).ToList();

        var games = (from x in heroesEntity.Replays
            where ids.Contains(x.GameMode)
            select x).ToLookup(x => x.GameMode);

        q = q.Where(x => games.Contains(x.Id)).ToList();

        foreach (var item in q)
        {
            item.BuildFirst = games[item.Id].Min(z => z.ReplayBuild);
            item.BuildLast = games[item.Id].Max(z => z.ReplayBuild);
            item.DateFrom = games[item.Id].Min(z => z.TimestampReplay);
            item.DateTo = games[item.Id].Max(z => z.TimestampReplay);
        }

        return q;
    }

    public static List<GameVersion> GetGameVersions(bool includeOnePrevious = false)
    {
        var vers = GetAllGameVersions();

        IEnumerable<GameVersion> Pick()
        {
            var count = 0;
            (int major, int minor) lastVer = default;
            GameVersion? latest = null;
            foreach (var ver in vers)
            {
                var key = (ver.Version.major, ver.Version.minor);
                if (!lastVer.Equals(key))
                {
                    count++;
                    lastVer = key;
                }

                if (count < 3 || includeOnePrevious)
                {
                    /* If this is the latest minor patch, split it to build numbers and return them individually:
                     *   2.54.2.85894, 2.54.2.85576, 2.54.2.#####, ...
                     * Otherwise, return a version which groups all build numbers in the minor patch:
                     *   2.54.1, 2.54.0, 2.53.1, ...
                     */
                    if (!latest.HasValue)
                    {
                        latest = ver;
                        var split = ver.SplitMinorPatch();
                        foreach (var splt in split)
                        {
                            yield return splt;
                        }
                    }
                    else
                    {
                        yield return ver;
                    }
                }

                if (count == 3)
                {
                    yield break;
                }
            }
        }

        return Pick().ToList();
    }

    // Retrieve list of game versions, most recent first
    private static List<GameVersion> GetAllGameVersions()
    {
        using var scope = _svcp.CreateScope();
        var heroesEntity = HeroesdataContext.Create(scope);

        (int major, int minor, int patch, int build)? ParseVersion(BuildNumber bldnum)
        {
            try
            {
                var parts = bldnum.Version.Split('.');
                var major = int.Parse(parts[0]);
                var minor = int.Parse(parts[1]);
                var patch = int.Parse(parts[2]);
                var build = int.Parse(parts[3]);
                return (major, minor, patch, build);
            }
            catch
            {
                return null;
            }
        }

        var bldnums = heroesEntity.BuildNumbers.ToList();

        var q = (from x in bldnums
                where x.Builddate.HasValue && x.Version != null
                let maybeVersion = ParseVersion(x)
                where maybeVersion.HasValue
                let version = maybeVersion.Value
                group x by (version.major, version.minor, version.patch)
                into grp1
                let buildFirst = grp1.Min(x => x.Buildnumber1)
                let buildLast = grp1.Max(x => x.Buildnumber1)
                select new GameVersion
                {
                    Version = grp1.Key,
                    BuildFirst = buildFirst,
                    BuildLast = buildLast,
                    Builds = grp1.OrderByDescending(z => z.Buildnumber1).Select(z => z.Buildnumber1).ToList(),
                    Title = $"Patch {grp1.Key.major}.{grp1.Key.minor}.{grp1.Key.patch}",
                })
            .OrderByDescending(x => x.Version)
            .ToList();

        return q;
    }
}
