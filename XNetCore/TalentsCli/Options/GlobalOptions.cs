using CommandLine;

namespace TalentsCli.Options;

public class GlobalOptions
{
    [Option('o', "output", Required = false, Default = OutputType.Human)]
    public OutputType OutputType { get; set; }
}
