using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace HighlightDetection.Test;

[Collection("Highlight Detector")]
public class TestDetectors
{
    private readonly HighlightDetectorFixture _fixture;
    private readonly TimeSpan _tenSec = TimeSpan.FromSeconds(10);

    public TestDetectors(HighlightDetectorFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void TestVallaHungeringArrowLastBounce()
    {
        var detector = new VallaHungeringArrowLastBounce();

        TestForLimitedDuration(
            detector,
            _tenSec,
            (replayFilePath, timestamps, isValid) =>
            {
                // Perform additional validation here
                if (isValid) { }
            });
    }

    private void TestForLimitedDuration(
        IHighlightDetector detector,
        TimeSpan duration,
        Action<string, IEnumerable<int>, bool> testOne)
    {
        var sw = new Stopwatch();
        sw.Start();
        foreach (var replayFilePath in _fixture.ReplayFiles)
        {
            // Stop the test after 10 seconds...
            if (sw.Elapsed >= duration)
            {
                break;
            }

            var replayFile = File.ReadAllBytes(replayFilePath);
            if (!detector.IsValid(replayFile))
            {
                testOne(replayFilePath, null, false);
                continue;
            }

            var timestamps = detector.DetectAndReturnTimestamps();

            testOne(replayFilePath, timestamps, true);
        }
    }
}
