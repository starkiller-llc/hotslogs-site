using CommandLine;

namespace TalentsCli.Options;

[Verb("delete", HelpText = "Delete a talent")]
public class DeleteOptions : GlobalOptions
{
    [Option('d', "dry-run", Required = false, Default = true, HelpText = "Dry run - show changes but don't commit")]
    public bool? DryRun { get; set; }

    [Option('h', "hero", Required = true, HelpText = "Hero name to insert talent for")]
    public string Hero { get; set; }

    [Option('t', "talent", Required = true, HelpText = "Talent id to insert")]
    public int TalentId { get; set; }

    [Option('b', "build", Required = true, HelpText = "Builds number (must be a beginning of a range)")]
    public int Build { get; set; }
}
