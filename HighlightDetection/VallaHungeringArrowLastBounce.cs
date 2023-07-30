using System.Collections.Generic;

namespace HighlightDetection;

public class VallaHungeringArrowLastBounce : HighlightDetectorBase
{
    public override IEnumerable<int> DetectAndReturnTimestamps()
    {
        return Empty;
    }

    protected override bool OnIsValid()
    {
        return HasHero("Valla");
    }
}
