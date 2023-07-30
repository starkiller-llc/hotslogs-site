using Heroes.ReplayParser;
using Heroes.ReplayParser.MPQFiles;
using System.Linq;
using Xunit;

namespace HighlightDetection.Test;

public class ChatExtract
{
    [Fact]
    public void ExtractChat()
    {
        var fn = @"d:\downloads\2022-07-24 12.42.17 은빛 도시.StormReplay";
        var r = DataParser.ParseReplay(fn, false, ParseOptions.FullParsing);
        var cht = r.Item2.GameEvents.Where(x => x.eventType == GameEventType.CTriggerChatMessageEvent).ToList();
        var cht2 = cht.Select(r => (Speaker: r.player.Name, Text: r.data.blobText)).ToList();
        var cht3 = string.Join("\n", cht2.Select(r => $"{r.Speaker}: {r.Text}"));
        var cht4 = cht2.Select(r => r.Speaker).Distinct().ToList();
        var cht5 = cht.Where(x => x.isGlobal).ToList();
    }
}
