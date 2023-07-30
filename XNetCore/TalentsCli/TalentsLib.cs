using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using TalentsCli.Options;

namespace TalentsCli;

public static class TalentsLib
{
    private static readonly string BadChars = Regex.Escape("- '‘’:,!.\"”?()");

    public static bool DeleteInternal(DeleteOptions opts, HeroesdataContext dc)
    {
        var q = dc.HeroTalentInformations
            .Where(x => x.ReplayBuildFirst == opts.Build)
            .Where(x => x.Character == opts.Hero)
            .AsQueryable();

        var talents = q.ToList();

        if (talents.All(x => x.ReplayBuildFirst != opts.Build))
        {
            var range = dc.HeroTalentInformations
                .Where(x => x.Character == opts.Hero)
                .FirstOrDefault(x => x.ReplayBuildFirst <= opts.Build && x.ReplayBuildLast >= opts.Build);
            Console.Error.Write($"Build {opts.Build} doesn't match the start of a build range, ");
            Console.Error.Write(
                range != null
                    ? $"it falls in range {range.ReplayBuildFirst}-{range.ReplayBuildLast}."
                    : "and no range contains it.");
            {
                return false;
            }
        }

        var replacedTalents = new List<HeroTalentInformation>();
        foreach (var t in talents.Where(x => x.TalentId >= opts.TalentId))
        {
            dc.HeroTalentInformations.Remove(t);
            if (t.TalentId == opts.TalentId)
            {
                continue;
            }

            var replacedTalent = new HeroTalentInformation
            {
                ReplayBuildFirst = t.ReplayBuildFirst,
                ReplayBuildLast = t.ReplayBuildLast,
                Character = t.Character,
                TalentId = t.TalentId - 1,
                TalentDescription = t.TalentDescription,
                TalentTier = t.TalentTier,
                TalentName = t.TalentName,
            };
            replacedTalents.Add(replacedTalent);
        }

        dc.HeroTalentInformations.AddRange(replacedTalents);
        return true;
    }

    public static Dictionary<int, List<DiffEntry>> DiffInternal(DiffOptions opts, HeroesdataContext dc)
    {
        var q = dc.HeroTalentInformations.AsQueryable();
        if (!string.IsNullOrEmpty(opts.Hero))
        {
            q = q.Where(x => x.Character == opts.Hero);
        }

        if (opts.MinBuild.HasValue)
        {
            var minBuild = opts.MinBuild.Value;
            q = q.Where(x => x.ReplayBuildLast >= minBuild);
        }

        if (opts.MaxBuild.HasValue)
        {
            var maxBuild = opts.MaxBuild.Value;
            q = q.Where(x => x.ReplayBuildFirst <= maxBuild);
        }

        if (opts.TalentId.HasValue)
        {
            var talentId = opts.TalentId.Value;
            q = q.Where(x => x.TalentId == talentId);
        }

        var talents = q.ToList();
        var byHeroByRange = talents
            .ToLookup(x => x.Character)
            .ToDictionary(x => x.Key, x => x.GroupBy(y => y.ReplayBuildFirst));

        var diffList = new Dictionary<int, List<DiffEntry>>();

        foreach (var kvp in byHeroByRange)
        {
            foreach (var range in kvp.Value.Triplewise((a, b, c) => (a, b, c)))
            {
                foreach (var t in range.b)
                {
                    var matchPrev = range.a?.SingleOrDefault(x => x.TalentName == t.TalentName);
                    var matchNext1 = range.c?.Where(x => x.TalentName == t.TalentName).ToList();
                    if (matchNext1.Count > 1)
                    {
                        matchNext1 = matchNext1.Where(x => x.TalentTier == t.TalentTier).ToList();
                    }

                    var matchNext = matchNext1.SingleOrDefault();

                    if (matchNext == null)
                    {
                        var extendedToNext = range.c?.Any(x => x.ReplayBuildLast == t.ReplayBuildLast) ?? false;
                        if (t.ReplayBuildLast == 1000000 || extendedToNext)
                        {
                            matchNext = t;
                        }
                    }

                    DiffEntry diffEntry = null;

                    void SetDiffExists(Action<DiffEntry> act)
                    {
                        if (diffEntry == null)
                        {
                            var nextRange = matchNext?.ReplayBuildFirst ?? range.c?.FirstOrDefault()?.ReplayBuildFirst;
                            diffEntry = new DiffEntry
                            {
                                Talent = t,
                                NextTalent = matchNext,
                                NextRange = nextRange,
                            };
                        }

                        act(diffEntry);
                    }

                    if (range.a != null && matchPrev == null)
                    {
                        SetDiffExists(x => x.IsNew = true);
                    }

                    if (matchNext != null)
                    {
                        if (matchNext.TalentTier != t.TalentTier)
                        {
                            SetDiffExists(x => x.IsTierChanged = true);
                        }

                        if (matchNext.TalentDescription != t.TalentDescription &&
                            opts.OutputType == OutputType.Extended)
                        {
                            SetDiffExists(x => x.IsDescriptionChanged = true);
                        }
                    }
                    else
                    {
                        SetDiffExists(x => x.IsRemovedInNext = true);
                    }

                    if (diffEntry != null)
                    {
                        if (!diffList.ContainsKey(t.ReplayBuildFirst))
                        {
                            diffList[t.ReplayBuildFirst] = new List<DiffEntry>();
                        }

                        diffList[t.ReplayBuildFirst].Add(diffEntry);
                    }
                }
            }
        }

        return diffList;
    }

    public static void ExtendInternal(ExtendOptions opts, HeroesdataContext dc)
    {
        var builds = dc.BuildNumbers.Select(x => x.Buildnumber1).OrderBy(x => x).ToList();
        var q = dc.HeroTalentInformations
            .Where(
                x => (x.ReplayBuildFirst <= opts.MinBuild && x.ReplayBuildLast >= opts.MinBuild) ||
                     (x.ReplayBuildFirst <= opts.MaxBuild && x.ReplayBuildFirst > opts.MinBuild))
            .AsQueryable();

        if (!string.IsNullOrEmpty(opts.Hero))
        {
            q = q.Where(x => x.Character == opts.Hero);
        }

        if (opts.TalentId.HasValue)
        {
            var talentId = opts.TalentId.Value;
            q = q.Where(x => x.TalentId == talentId);
        }

        var talents = q.ToList();

        var startTalents = talents
            .Where(x => x.ReplayBuildFirst <= opts.MinBuild && x.ReplayBuildLast >= opts.MinBuild).ToList();

        var toRemove = talents
            .Except(startTalents)
            .Where(x => x.ReplayBuildFirst <= opts.MaxBuild && x.ReplayBuildLast <= opts.MaxBuild).ToList();

        var toSplit = talents
            .Except(startTalents)
            .Where(x => x.ReplayBuildFirst <= opts.MaxBuild && x.ReplayBuildLast > opts.MaxBuild).ToList();

        var trueLast = builds.Last(x => x <= opts.MaxBuild);

        startTalents.ForEach(x => { x.ReplayBuildLast = trueLast; });

        dc.HeroTalentInformations.RemoveRange(toRemove);

        var nextBuild = builds.First(x => x > opts.MaxBuild);

        foreach (var t in toSplit)
        {
            var newTalent = new HeroTalentInformation
            {
                ReplayBuildFirst = nextBuild,
                ReplayBuildLast = t.ReplayBuildLast,
                Character = t.Character,
                TalentId = t.TalentId,
                TalentDescription = t.TalentDescription,
                TalentTier = t.TalentTier,
                TalentName = t.TalentName,
            };
            dc.HeroTalentInformations.Add(newTalent);
        }

        dc.HeroTalentInformations.RemoveRange(toSplit);
    }

    public static void FixTalentImagePathsInternal(FixTalentImagePathsOptions opts, HeroesdataContext dc)
    {
        var talentImageMappings = dc.TalentImageMappings.ToList();
        foreach (var mapping in talentImageMappings)
        {
            var rawFileName = $"{mapping.Character}{mapping.TalentName}";
            mapping.TalentImage = $"~/images/talents/{PrepareForImageURL(rawFileName)}.png";
        }
    }

    public static List<HeroTalentInformation> GetInternal(GetOptions opts, HeroesdataContext dc)
    {
        var latest = false;
        var q = dc.HeroTalentInformations.AsQueryable();
        if (!string.IsNullOrEmpty(opts.Hero))
        {
            q = q.Where(x => x.Character == opts.Hero);
        }

        if (!string.IsNullOrEmpty(opts.Build))
        {
            if (opts.Build == "latest")
            {
                latest = true;
            }
            else
            {
                var build = opts.Build == "current" ? 1000000 : int.Parse(opts.Build);
                q = q.Where(x => x.ReplayBuildFirst <= build && x.ReplayBuildLast >= build);
            }
        }

        if (opts.MinBuild.HasValue)
        {
            var minBuild = opts.MinBuild.Value;
            q = q.Where(x => x.ReplayBuildLast >= minBuild);
        }

        if (opts.MaxBuild.HasValue)
        {
            var maxBuild = opts.MaxBuild.Value;
            q = q.Where(x => x.ReplayBuildFirst <= maxBuild);
        }

        if (opts.TalentId.HasValue)
        {
            var talentId = opts.TalentId.Value;
            q = q.Where(x => x.TalentId == talentId);
        }

        var talents = q.ToList();

        if (latest)
        {
            var g = talents.GroupBy(
                    x => new
                    {
                        x.Character,
                        x.TalentId,
                    },
                    x => x.ReplayBuildLast)
                .ToDictionary(x => (x.Key.Character, x.Key.TalentId), x => x.Max());

            talents = talents.Where(x => x.ReplayBuildLast == g[(x.Character, x.TalentId)]).ToList();
        }

        return talents;
    }

    public static bool InsertInternal(InsertOptions opts, HeroesdataContext dc)
    {
        var q = dc.HeroTalentInformations
            .Where(
                x => x.ReplayBuildFirst == opts.Build || (opts.IncludeLater.Value && x.ReplayBuildFirst > opts.Build))
            .Where(x => x.Character == opts.Hero)
            .AsQueryable();

        var talents = q.ToList();

        if (talents.All(x => x.ReplayBuildFirst != opts.Build))
        {
            var range = dc.HeroTalentInformations
                .Where(x => x.Character == opts.Hero)
                .FirstOrDefault(x => x.ReplayBuildFirst <= opts.Build && x.ReplayBuildLast >= opts.Build);
            Console.Error.Write($"Build {opts.Build} doesn't match the start of a build range, ");
            Console.Error.Write(
                range != null
                    ? $"it falls in range {range.ReplayBuildFirst}-{range.ReplayBuildLast}."
                    : "and no range contains it.");
            {
                return false;
            }
        }

        var replacedTalents = new List<HeroTalentInformation>();
        foreach (var t in talents.Where(x => x.TalentId >= opts.TalentId))
        {
            dc.HeroTalentInformations.Remove(t);
            var replacedTalent = new HeroTalentInformation
            {
                ReplayBuildFirst = t.ReplayBuildFirst,
                ReplayBuildLast = t.ReplayBuildLast,
                Character = t.Character,
                TalentId = t.TalentId + 1,
                TalentDescription = t.TalentDescription,
                TalentTier = t.TalentTier,
                TalentName = t.TalentName,
            };
            replacedTalents.Add(replacedTalent);
        }

        dc.HeroTalentInformations.AddRange(replacedTalents);

        var ranges = talents
            .Select(x => (x.ReplayBuildFirst, x.ReplayBuildLast))
            .Distinct()
            .GroupBy(x => x.ReplayBuildFirst)
            .ToDictionary(x => x.Key, x => x.Max(y => y.ReplayBuildLast))
            .Select(kvp => (ReplayBuildFirst: kvp.Key, ReplayBuildLast: kvp.Value));
        foreach ((var replayBuildFirst, var replayBuildLast) in ranges)
        {
            var newTalent = new HeroTalentInformation
            {
                ReplayBuildFirst = replayBuildFirst,
                ReplayBuildLast = replayBuildLast,
                Character = opts.Hero,
                TalentId = opts.TalentId,
                TalentDescription = opts.TalentDescription,
                TalentTier = opts.TalentTier,
                TalentName = opts.TalentName,
            };
            dc.HeroTalentInformations.Add(newTalent);
        }

        return true;
    }

    public static void NewBuildInternal(NewBuildOptions opts, HeroesdataContext dc)
    {
        int[] tierLevels = { 1, 4, 7, 10, 13, 16, 20 };
        var ser = new XmlSerializer(typeof(TalentUpdatePackage));
        using var f = File.OpenText(opts.Path);
        var tup = (TalentUpdatePackage)ser.Deserialize(f);
        var talentInfoList = tup.TalentInfoList;

        SaveTalentImages(opts, talentInfoList);

        var alarakCounterStrike20 =
            talentInfoList.Single(x => x.HeroName == "Alarak" && x.Tier == 7 && x.TalentName == "Counter-Strike");
        alarakCounterStrike20.TalentName += " (Lvl 20)";
        var alarakDeadlyCharge20 =
            talentInfoList.Single(x => x.HeroName == "Alarak" && x.Tier == 7 && x.TalentName == "Deadly Charge");
        alarakDeadlyCharge20.TalentName += " (Lvl 20)";

        var currentTalents = GetInternal(
            new GetOptions
            {
                Build = "current",
            },
            dc);
        var characters = talentInfoList.Select(x => x.HeroName).Distinct().ToList();
        var dicCurrentRef = currentTalents.ToDictionary(x => (x.Character, x.TalentId));
        var dicCurrent = currentTalents.ToDictionary(
            x => (x.Character, x.TalentId),
            x => (x.TalentName, x.TalentTier, x.TalentDescription));
        var dicNewRef = talentInfoList.ToDictionary(x => (x.HeroName, x.TalentId));
        var dicNew = talentInfoList.ToDictionary(
            x => (x.HeroName, x.TalentId),
            x => (x.TalentName, TalentTier: tierLevels[x.Tier - 1], x.TalentDescription));
        var joinKeys = dicCurrent.Keys.Intersect(dicNew.Keys).ToList();
        var unchanged = joinKeys.Where(x => dicNew[x] == dicCurrent[x]).ToList();
        var changed = joinKeys.Where(x => dicNew[x] != dicCurrent[x]).ToList();
        var newTalents = dicNew.Keys.Except(dicCurrent.Keys);
        var removedTalents = dicCurrent.Keys.Except(dicNew.Keys);

        var builds = dc.BuildNumbers.OrderBy(x => x).ToList();

        if (builds.All(x => x.Buildnumber1 != tup.Build))
        {
            var newBuildNumber = new BuildNumber
            {
                Builddate = tup.Date,
                Buildnumber1 = tup.Build,
                Version = tup.Version,
            };
            dc.BuildNumbers.Add(newBuildNumber);
        }

        var lastBuild = builds.Last(x => x.Buildnumber1 < tup.Build).Buildnumber1;

        var imageMappings = dc.TalentImageMappings.Select(x => x.TalentName.ToLowerInvariant()).Distinct().ToHashSet();

        foreach ((string, int) v in changed)
        {
            var dbTalent = dicCurrentRef[v];
            var newRef = dicNewRef[v];
            dbTalent.ReplayBuildLast = lastBuild;
            var dbUpdateTalent = new HeroTalentInformation
            {
                TalentDescription = newRef.TalentDescription,
                TalentTier = tierLevels[newRef.Tier - 1],
                TalentName = newRef.TalentName,
                TalentId = newRef.TalentId,
                Character = newRef.HeroName,
                ReplayBuildFirst = tup.Build,
                ReplayBuildLast = 1000000,
            };

            var t = dbUpdateTalent;
            if (!imageMappings.Contains(t.TalentName.ToLowerInvariant()))
            {
                var rawFileName = $"{t.Character}{t.TalentName}";
                var newImageMapping = new TalentImageMapping
                {
                    Character = t.Character,
                    HeroName = t.Character,
                    TalentName = t.TalentName,
                    TalentImage = $"/images/talents/{PrepareForImageURL(rawFileName)}.png",
                };
                Console.WriteLine(
                    $"Added image mapping for {newImageMapping.TalentName} to {newImageMapping.TalentImage}");
                dc.TalentImageMappings.Add(newImageMapping);
            }

            dc.HeroTalentInformations.Add(dbUpdateTalent);
        }

        foreach ((string, int) v in newTalents)
        {
            var newRef = dicNewRef[v];
            var dbTalent = new HeroTalentInformation
            {
                TalentDescription = newRef.TalentDescription,
                TalentTier = tierLevels[newRef.Tier - 1],
                TalentName = newRef.TalentName,
                TalentId = newRef.TalentId,
                Character = newRef.HeroName,
                ReplayBuildFirst = tup.Build,
                ReplayBuildLast = 1000000,
            };

            var t = dbTalent;
            if (!imageMappings.Contains(t.TalentName.ToLowerInvariant()))
            {
                var rawFileName = $"{t.Character}{t.TalentName}";
                var newImageMapping = new TalentImageMapping
                {
                    Character = t.Character,
                    HeroName = t.Character,
                    TalentName = t.TalentName,
                    TalentImage = $"~/images/talents/{PrepareForImageURL(rawFileName)}.png",
                };
                Console.WriteLine(
                    $"Added image mapping for {newImageMapping.TalentName} to {newImageMapping.TalentImage}");
                dc.TalentImageMappings.Add(newImageMapping);
            }

            dc.HeroTalentInformations.Add(dbTalent);
        }

        foreach ((string, int) v in removedTalents)
        {
            var dbTalent = dicCurrentRef[v];
            dbTalent.ReplayBuildLast = lastBuild;
        }
    }

    public static string PrepareForImageURL(this string input)
    {
        if (input == null)
        {
            return null;
        }

        return Regex.Replace(input, $"[{BadChars}]", "");
    }

    public static void SplitInternal(SplitOptions opts, HeroesdataContext dc)
    {
        // say we got a range of 0-2000 and we want to split it to 0-400 and 500-2000
        // we want all ranges that include 400 and 500, so all ranges where first<400
        // or last>500. So if we got a range of 400-450 we dont touch it but if we
        // got a range of 400-600 we want 400-400 and 500-600

        var q = dc.HeroTalentInformations
            .Where(
                x =>
                    (x.ReplayBuildFirst < opts.MinBuild && x.ReplayBuildLast > opts.MinBuild) ||
                    (x.ReplayBuildFirst < opts.MaxBuild && x.ReplayBuildLast > opts.MaxBuild))
            .AsQueryable();

        if (!string.IsNullOrEmpty(opts.Hero))
        {
            q = q.Where(x => x.Character == opts.Hero);
        }

        if (opts.TalentId.HasValue)
        {
            var talentId = opts.TalentId.Value;
            q = q.Where(x => x.TalentId == talentId);
        }

        var talents = q.ToList();

        var newTalents = talents.Select(
            x => new HeroTalentInformation
            {
                ReplayBuildFirst = opts.MaxBuild,
                ReplayBuildLast = x.ReplayBuildLast,
                Character = x.Character,
                TalentId = x.TalentId,
                TalentDescription = x.TalentDescription,
                TalentTier = x.TalentTier,
                TalentName = x.TalentName,
            });

        dc.HeroTalentInformations.AddRange(newTalents);

        talents.ForEach(x => { x.ReplayBuildLast = opts.MinBuild; });
    }

    private static void SaveTalentImages(NewBuildOptions opts, List<TalentInfo> talentInfoList)
    {
        var targetDir = opts.TargetImagePath;
        foreach (var talentInfo in talentInfoList)
        {
            var ddsBytes = talentInfo.Image;
            var ddsImage = new DDSImage(ddsBytes, true);
            var talentName = talentInfo.TalentName;
            var heroName = talentInfo.HeroName;
            var rawFileName = $"{heroName}{talentName}";
            var imageName = $"{PrepareForImageURL(rawFileName)}.png";
            var path = Path.Combine(targetDir, imageName);
            if (opts.DryRun ?? false)
            {
                Console.WriteLine($"Would save image {path}");
            }
            else
            {
                Console.WriteLine($"Saving image {path}");
                ddsImage.Save(path);
            }
        }
    }
}
