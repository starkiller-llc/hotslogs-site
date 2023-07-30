using CommandLine;

namespace TalentsCli.Options;

[Verb("insert", HelpText = "Insert a talent")]
public class InsertOptions : GlobalOptions
{
    [Option('d', "dry-run", Required = false, Default = true, HelpText = "Dry run - show changes but don't commit")]
    public bool? DryRun { get; set; }

    [Option('h', "hero", Required = true, HelpText = "Hero name to insert talent for")]
    public string Hero { get; set; }

    [Option('t', "talent", Required = true, HelpText = "Talent id to insert")]
    public int TalentId { get; set; }

    [Option('b', "build", Required = true, HelpText = "Builds number (must be a beginning of a range)")]
    public int Build { get; set; }

    [Option('i', "include-later", Required = true, HelpText = "Include all subsequent build ranges")]
    public bool? IncludeLater { get; set; }

    [Option('m', "talent-name", Required = true, HelpText = "Inserted talent name")]
    public string TalentName { get; set; }

    [Option(
        'r',
        "talent-tier",
        Required = true,
        HelpText = "Inserted talent tier (1,4,7,10,13,16 or 20) - even for Chromie")]
    public int TalentTier { get; set; }

    [Option('v', "talent-description", Required = true, HelpText = "Inserted talent description")]
    public string TalentDescription { get; set; }
}
