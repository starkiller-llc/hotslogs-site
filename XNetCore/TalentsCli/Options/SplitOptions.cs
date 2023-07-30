using CommandLine;

namespace TalentsCli.Options;

[Verb("split", HelpText = "Split talent range")]
public class SplitOptions : GlobalOptions
{
    [Option('d', "dry-run", Required = false, Default = true, HelpText = "Dry run - show changes but don't commit")]
    public bool? DryRun { get; set; }

    [Option('h', "hero", Required = false, HelpText = "Split for hero")]
    public string Hero { get; set; }

    [Option('t', "talent", Required = false, HelpText = "Split for talent id")]
    public int? TalentId { get; set; }

    [Option('n', "old-build", Required = true, HelpText = "Build to split")]
    public int MinBuild { get; set; }

    [Option('x', "new-build", Required = true, HelpText = "New build number")]
    public int MaxBuild { get; set; }
}
