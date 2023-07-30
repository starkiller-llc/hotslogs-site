using HotsLogsApi.BL.Migration;
using HotsLogsApi.BL.Migration.HeroAndMap;
using Microsoft.Extensions.DependencyInjection;

namespace HotsLogsApi.BL.Test;

public class UnitTest1
{
    private readonly List<int> _i0;
    private readonly List<string> _s0;
    private readonly ServiceProvider _svcp;

    public UnitTest1()
    {
        _s0 = new List<string>();
        _i0 = new List<int>();
        var services = new ServiceCollection();

        // TODO: add services for tests

        _svcp = services.BuildServiceProvider();
    }

    [Fact]
    public void TestHeroAndMapDefaultArgs()
    {
        var args = new HeroAndMapArgs
        {
            GameMode = "8",
            League = _i0,
            Hero = "Abathur",
            Map = _s0,
            Time = _s0,
            GameModeEx = "8",
            Tournament = "1156",
            Patch = _s0,
            GameLength = _i0,
            Level = _s0,
            Talent = "AllTalents",
        };
        var helper = new Helper(args, _svcp);
        var res = helper.MainCalculation();
    }

    [Fact]
    public void TestHeroAndMapMostPopular()
    {
        var args = new HeroAndMapArgs
        {
            GameMode = "8",
            League = _i0,
            Hero = "Abathur",
            Map = _s0,
            Time = _s0,
            GameModeEx = "8",
            Tournament = "1156",
            Patch = _s0,
            GameLength = _i0,
            Level = _s0,
            Talent = "MostPopular",
        };
        var helper = new Helper(args, _svcp);
        var res = helper.MainCalculation();
    }

    [Fact]
    public void TestHeroAndMapMostPopularGameLength()
    {
        var args = new HeroAndMapArgs
        {
            GameMode = "8",
            League = _i0,
            Hero = "Abathur",
            Map = _s0,
            Time = _s0,
            GameModeEx = "8",
            Tournament = "1156",
            Patch = _s0,
            GameLength = new List<int> { 10 },
            Level = _s0,
            Talent = "MostPopular",
        };
        var helper = new Helper(args, _svcp);
        var res = helper.MainCalculation();
    }
}
