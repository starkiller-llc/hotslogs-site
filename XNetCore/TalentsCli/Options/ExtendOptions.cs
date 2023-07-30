using CommandLine;

namespace TalentsCli.Options;

[Verb("extend", HelpText = "Extend range with overwrite")]
public class ExtendOptions : GlobalOptions
{
    [Option('d', "dry-run", Required = false, Default = true, HelpText = "Dry run - show changes but don't commit")]
    public bool? DryRun { get; set; }

    [Option('h', "hero", Required = false, HelpText = "Extend for hero")]
    public string Hero { get; set; }

    [Option('t', "talent", Required = false, HelpText = "Extend for talent id")]
    public int? TalentId { get; set; }

    [Option('n', "from-build", Required = true, HelpText = "Start of build range")]
    public int MinBuild { get; set; }

    [Option('x', "to-build", Required = true, HelpText = "End of new build range")]
    public int MaxBuild { get; set; }
}
