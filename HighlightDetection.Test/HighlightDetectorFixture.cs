using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HighlightDetection.Test;

public class HighlightDetectorFixture
{
    public const string ReplayExtensionFilter = "*.StormReplay";

    public HighlightDetectorFixture()
    {
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Heroes of the Storm\\Accounts");
        ReplayFiles = Directory.GetFiles(baseDir, ReplayExtensionFilter, SearchOption.AllDirectories)
            .Where(i => Path.GetDirectoryName(i)?.EndsWith("Replays\\Multiplayer") ?? false)
            .ToList();
    }

    public List<string> ReplayFiles { get; }
}
