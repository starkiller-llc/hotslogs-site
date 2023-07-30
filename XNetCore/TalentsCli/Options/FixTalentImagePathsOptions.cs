using CommandLine;

namespace TalentsCli.Options;

[Verb("fix-talent-image-mappings", HelpText = "Fix talent image mappings to new format (<hero><talent>.png)")]
public class FixTalentImagePathsOptions
{
    [Option('d', "dry-run", Required = false, Default = true, HelpText = "Dry run - show changes but don't commit")]
    public bool? DryRun { get; set; }
}
