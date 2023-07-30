using Heroes.ReplayParser;
using System;
using System.Collections.Generic;
using System.Linq;
using static Heroes.ReplayParser.DataParser;

namespace HighlightDetection;

public abstract class HighlightDetectorBase : IHighlightDetector
{
    protected readonly IReadOnlyList<int> Empty = new List<int>();

    public ParseOptions ValidationParseLevel { get; protected set; } = ParseOptions.MinimalParsing;
    public ParseOptions DetectionParseLevel { get; protected set; } = ParseOptions.FullParsing;
    protected Replay Replay { get; set; }

    public bool IsValid(byte[] replayFile)
    {
        var (status, replay) = ParseReplay(replayFile, ValidationParseLevel);

        Replay = replay;

        if (status != ReplayParseResult.Success)
        {
            return false;
        }

        if (!OnIsValid())
        {
            return false;
        }

        if (ValidationParseLevel == DetectionParseLevel)
        {
            return true;
        }

        (status, Replay) = ParseReplay(replayFile, DetectionParseLevel);

        return status == ReplayParseResult.Success;
    }

    public abstract IEnumerable<int> DetectAndReturnTimestamps();

    protected bool HasHero(string hero)
    {
        return Replay.Players.Any(x => x.HeroId == hero || x.HeroAttributeId == hero);
    }

    protected virtual bool OnIsValid() { return true; }
}
