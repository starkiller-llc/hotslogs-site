using CommandLine;

namespace TalentsCli.Options;

[Verb("new-build", HelpText = "Create new talent build")]
public class NewBuildOptions : GlobalOptions
{
    [Option('d', "dry-run", Required = false, Default = true, HelpText = "Dry run - show changes but don't commit")]
    public bool? DryRun { get; set; }

    [Option('i', "input", Required = true, HelpText = "Input file of new talents (XML)")]
    public string Path { get; set; }

    [Option(
        'p',
        "target-image-path",
        Required = false,
        HelpText = "Path to web talent images folder",
        Default = @"D:\StarKillerLLC\HOTSLogs\HotsLogsApi\wwwroot\Images\Talents")]
    public string TargetImagePath { get; set; }
}
