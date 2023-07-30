using CommandLine;

namespace TalentsCli.Options;

[Verb("diff", HelpText = "Report differences across range")]
public class DiffOptions : GlobalOptions
{
    [Option('h', "hero", Required = false, HelpText = "Filter by hero")]
    public string Hero { get; set; }

    [Option('t', "talent", Required = false, HelpText = "Filter by talent id")]
    public int? TalentId { get; set; }

    [Option('n', "min-build", Required = false, HelpText = "Filter by builds of at least some value")]
    public int? MinBuild { get; set; }

    [Option('x', "max-build", Required = false, HelpText = "Filter by builds of at most some value")]
    public int? MaxBuild { get; set; }
}
