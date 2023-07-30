using CommandLine;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using TalentsCli.Options;

namespace TalentsCli;

internal class Program
{
    private static DbContextOptions<HeroesdataContext> _options;

    private static void ConfirmSave(DbContext dc)
    {
        if (dc.ChangeTracker.Entries().All(x => x.State == EntityState.Unchanged))
        {
            Console.WriteLine("Nothing to do.");
            return;
        }

        Console.Write("Apply above changes? [y/n] ");
        var key = Console.ReadKey();
        Console.WriteLine();
        if (key.Key == ConsoleKey.N)
        {
            Console.WriteLine("Not changing anything.");
        }
        else
        {
            Console.WriteLine("Comitting...");
            dc.SaveChanges();
            Console.WriteLine("Done.");
        }
    }

    private static string Describe(HeroTalentInformation e)
    {
        return $"{e.Character} {e.TalentId} {e.TalentName}({e.TalentTier}) {e.ReplayBuildFirst}-{e.ReplayBuildLast}";
    }

    private static string Describe(TalentImageMapping e)
    {
        return $"Image Mapping: {e.Character} {e.TalentName} {e.TalentImage}";
    }

    private static string Describe(BuildNumber e)
    {
        return $"Build Number: {e.Builddate} {e.Buildnumber1} {e.Version}";
    }

    private static int Main(string[] args)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<Program>();
        var config = configBuilder.Build();
        var connectionString = config.GetConnectionString("DefaultConnection");
        var builder = new DbContextOptionsBuilder<HeroesdataContext>()
            .EnableSensitiveDataLogging()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        _options = builder.Options;

        return Parser.Default
            .ParseArguments<
                GetOptions,
                SplitOptions,
                InsertOptions,
                ExtendOptions,
                DeleteOptions,
                DiffOptions,
                NewBuildOptions,
                FixTalentImagePathsOptions
            >(args)
            .MapResult(
                (GetOptions opts) => RunGet(opts),
                (SplitOptions opts) => RunSplit(opts),
                (InsertOptions opts) => RunInsert(opts),
                (ExtendOptions opts) => RunExtend(opts),
                (DeleteOptions opts) => RunDelete(opts),
                (DiffOptions opts) => RunDiff(opts),
                (NewBuildOptions opts) => RunNewBuild(opts),
                (FixTalentImagePathsOptions opts) => RunFixTalentImagePaths(opts),
                errs => 1);
    }

    private static void PrintDiff(GlobalOptions opts, Dictionary<int, List<DiffEntry>> diffList)
    {
        var oneHero = diffList.SelectMany(x => x.Value).Select(x => x.Talent.Character).Distinct().Count() == 1;

        foreach (var range in diffList)
        {
            foreach (var diffEntry in range.Value)
            {
                var t = diffEntry.Talent;
                var nextRange = diffEntry.NextRange;
                var matchNext = diffEntry.NextTalent;
                if (!oneHero)
                {
                    Console.Write($"{t.Character} ");
                }

                Console.Write($"Build {t.ReplayBuildFirst}-{nextRange} talent {t.TalentName}: ");
                var anyOut = false;

                void Out(string msg)
                {
                    if (anyOut)
                    {
                        Console.Write(", ");
                    }

                    Console.Write(msg);
                    anyOut = true;
                }

                if (diffEntry.IsNew)
                {
                    Out("new");
                }

                if (diffEntry.IsTierChanged)
                {
                    Out($"tier {t.TalentTier} --> {matchNext.TalentTier}");
                }

                if (diffEntry.IsDescriptionChanged && opts.OutputType == OutputType.Extended)
                {
                    Out($"description {t.TalentDescription} --> {matchNext.TalentDescription}");
                }

                if (diffEntry.IsRemovedInNext)
                {
                    Out("removed in next");
                }

                if (anyOut)
                {
                    Console.WriteLine();
                }
            }

            Console.WriteLine("-----------");
        }
    }

    private static void PrintTalent(GlobalOptions opts, HeroTalentInformation t)
    {
        switch (opts.OutputType)
        {
            case OutputType.Human:
                Console.WriteLine($"{Describe(t)}");
                break;
            case OutputType.Extended:
                Console.WriteLine($"{Describe(t)}");
                Console.WriteLine($"   -->{t.TalentDescription}");
                break;
            case OutputType.Csv:
                break;
            case OutputType.Json:
                break;
            // ReSharper disable once RedundantCaseLabel
            case OutputType.ListRanges:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static int RunDelete(DeleteOptions opts)
    {
        using var dc = new HeroesdataContext(_options);
        if (!TalentsLib.DeleteInternal(opts, dc))
        {
            return 1;
        }

        ShowChanges(dc);

        if (!opts.DryRun.GetValueOrDefault(true))
        {
            ConfirmSave(dc);
        }

        return 0;
    }

    private static int RunDiff(DiffOptions opts)
    {
        using var dc = new HeroesdataContext(_options);

        var diffList = TalentsLib.DiffInternal(opts, dc);

        PrintDiff(opts, diffList);

        return 0;
    }

    private static int RunExtend(ExtendOptions opts)
    {
        using var dc = new HeroesdataContext(_options);

        TalentsLib.ExtendInternal(opts, dc);

        ShowChanges(dc);

        if (!opts.DryRun.GetValueOrDefault(true))
        {
            ConfirmSave(dc);
        }

        return 0;
    }

    private static int RunFixTalentImagePaths(FixTalentImagePathsOptions opts)
    {
        using var dc = new HeroesdataContext(_options);

        TalentsLib.FixTalentImagePathsInternal(opts, dc);

        ShowChanges(dc);

        if (!opts.DryRun.GetValueOrDefault(true))
        {
            ConfirmSave(dc);
        }

        return 0;
    }

    private static int RunGet(GetOptions opts)
    {
        using var dc = new HeroesdataContext(_options);
        var talents = TalentsLib.GetInternal(opts, dc);

        if (opts.OutputType == OutputType.ListRanges)
        {
            var ranges = talents.Select(x => (x.ReplayBuildFirst, x.ReplayBuildLast)).Distinct().ToList();
            foreach ((var replayBuildFirst, var replayBuildLast) in ranges)
            {
                Console.WriteLine($"Build {replayBuildFirst}-{replayBuildLast}");
            }
        }
        else
        {
            foreach (var t in talents)
            {
                PrintTalent(opts, t);
            }
        }

        return 0;
    }

    private static int RunInsert(InsertOptions opts)
    {
        using var dc = new HeroesdataContext(_options);
        if (!TalentsLib.InsertInternal(opts, dc))
        {
            return 1;
        }

        ShowChanges(dc);

        if (!opts.DryRun.GetValueOrDefault(true))
        {
            ConfirmSave(dc);
        }

        return 0;
    }

    private static int RunNewBuild(NewBuildOptions opts)
    {
        using var dc = new HeroesdataContext(_options);

        TalentsLib.NewBuildInternal(opts, dc);

        ShowChanges(dc);

        if (!opts.DryRun.GetValueOrDefault(true))
        {
            ConfirmSave(dc);
        }

        return 0;
    }

    private static int RunSplit(SplitOptions opts)
    {
        using var dc = new HeroesdataContext(_options);

        TalentsLib.SplitInternal(opts, dc);

        ShowChanges(dc);

        if (!opts.DryRun.GetValueOrDefault(true))
        {
            ConfirmSave(dc);
        }

        return 0;
    }

    private static void ShowChanges(DbContext dc)
    {
        var entries = dc.ChangeTracker.Entries<HeroTalentInformation>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Unchanged)
            {
                continue;
            }

            Console.WriteLine($"{entry.State} {Describe(entry.Entity)}");
        }

        var entries2 = dc.ChangeTracker.Entries<TalentImageMapping>();
        foreach (var entry in entries2)
        {
            if (entry.State == EntityState.Unchanged)
            {
                continue;
            }

            Console.WriteLine($"{entry.State} {Describe(entry.Entity)}");
        }

        var entries3 = dc.ChangeTracker.Entries<BuildNumber>();
        foreach (var entry in entries3)
        {
            if (entry.State == EntityState.Unchanged)
            {
                continue;
            }

            Console.WriteLine($"{entry.State} {Describe(entry.Entity)}");
        }
    }
}
