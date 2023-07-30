using System.Collections.Generic;

namespace HelperCore;

public struct GameVersion
{
    public (int major, int minor, int patch) Version { get; set; }
    public int BuildFirst { get; set; }
    public int BuildLast { get; set; }
    public List<int> Builds { get; set; }
    public string Title { get; set; }

    public IEnumerable<GameVersion> SplitMinorPatch()
    {
        foreach (var build in Builds)
        {
            yield return new GameVersion
            {
                Version = Version,
                BuildFirst = build,
                BuildLast = build,
                Builds = new List<int> { build },
                Title = $"Patch {Version.major}.{Version.minor}.{Version.patch}.{build}",
            };
        }
    }
}
