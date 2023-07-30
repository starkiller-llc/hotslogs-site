using System.Collections.Generic;

namespace HighlightDetection;

public interface IHighlightDetector
{
    IEnumerable<int> DetectAndReturnTimestamps();
    bool IsValid(byte[] replayFile);
}
