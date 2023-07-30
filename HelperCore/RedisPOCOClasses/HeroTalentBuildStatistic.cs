using System;
using System.Web;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class HeroTalentBuildStatistic
{
    public int GamesPlayed { get; set; }
    public decimal WinPercent { get; set; }
    public int GamesWon { get; set; }
    public int CharacterID { get; set; }
    public int? LeagueID { get; set; }
    public int MapID { get; set; }

    public string[] TalentNameDescription { get; set; } = new string[7];
    public string[] TalentName { get; set; } = new string[7];
    public string[] TalentImageURL { get; set; } = new string[7];
    public int[] TalentId { get; set; }

    public static HeroTalentBuildStatistic operator +(HeroTalentBuildStatistic a, HeroTalentBuildStatistic b)
    {
        if (a == null)
        {
            return b;
        }

        if (b == null)
        {
            return a;
        }

        if (a.GamesPlayed > 0 || b.GamesPlayed > 0)
        {
            a.WinPercent = ((a.GamesPlayed * a.WinPercent) + (b.GamesPlayed * b.WinPercent)) /
                           (a.GamesPlayed + b.GamesPlayed);
        }
        else
        {
            a.WinPercent = 0;
        }

        a.GamesPlayed += b.GamesPlayed;

        return a;
    }

    public string GetTalentBuildCardHTML(bool includeStormTalent = false)
    {
        const string TalentBuildCardHTMLTemplate =
            @"<div class=""talentBuildCard"">
                    <table class=""talentBuildCardHeader"">
                        <tr><td>Win Percent</td><td>{0}</td></tr>
                        <tr><td>Games Played</td><td>{1}</td></tr>
                    </table>
                    <table class=""talentBuildCardContent"">
                        <tr><td>1</td><td><img title=""{2}"" src=""{9}"" alt=""{2}""></td><td>{16}</td></tr>
                        <tr><td>4</td><td><img title=""{3}"" src=""{10}"" alt=""{3}""></td><td>{17}</td></tr>
                        <tr><td>7</td><td><img title=""{4}"" src=""{11}"" alt=""{4}""></td><td>{18}</td></tr>
                        <tr><td>10</td><td><img title=""{5}"" src=""{12}"" alt=""{5}""></td><td>{19}</td></tr>
                        <tr><td>13</td><td><img title=""{6}"" src=""{13}"" alt=""{6}""></td><td>{20}</td></tr>
                        <tr><td>16</td><td><img title=""{7}"" src=""{14}"" alt=""{7}""></td><td>{21}</td></tr>
                        <tr><td>20</td><td><img title=""{8}"" src=""{15}"" alt=""{8}""></td><td>{22}</td></tr>
                    </table>
                </div>";

        const string TalentBuildCardHTMLTemplateExcludeStormTalent =
            @"<div class=""talentBuildCard"">
                    <table class=""talentBuildCardHeader"">
                        <tr><td>Win Percent</td><td>{0}</td></tr>
                        <tr><td>Games Played</td><td>{1}</td></tr>
                    </table>
                    <table class=""talentBuildCardContent"">
                        <tr><td>1</td><td><img title=""{2}"" src=""{8}"" alt=""{2}""></td><td>{14}</td></tr>
                        <tr><td>4</td><td><img title=""{3}"" src=""{9}"" alt=""{3}""></td><td>{15}</td></tr>
                        <tr><td>7</td><td><img title=""{4}"" src=""{10}"" alt=""{4}""></td><td>{16}</td></tr>
                        <tr><td>10</td><td><img title=""{5}"" src=""{11}"" alt=""{5}""></td><td>{17}</td></tr>
                        <tr><td>13</td><td><img title=""{6}"" src=""{12}"" alt=""{6}""></td><td>{18}</td></tr>
                        <tr><td>16</td><td><img title=""{7}"" src=""{13}"" alt=""{7}""></td><td>{19}</td></tr>
                    </table>
                </div>";

        // TODO: This throws occasionally, and in general doesn't work well, debug -- Aviad 14-Jun-2020
        return !includeStormTalent
            ? string.Format(
                TalentBuildCardHTMLTemplateExcludeStormTalent,
                WinPercent.ToString("P1"),
                GamesPlayed,
                HttpUtility.HtmlEncode(TalentNameDescription[0]),
                HttpUtility.HtmlEncode(TalentNameDescription[1]),
                HttpUtility.HtmlEncode(TalentNameDescription[2]),
                HttpUtility.HtmlEncode(TalentNameDescription[3]),
                HttpUtility.HtmlEncode(TalentNameDescription[4]),
                HttpUtility.HtmlEncode(TalentNameDescription[5]),
                TalentImageURL[0],
                TalentImageURL[1],
                TalentImageURL[2],
                TalentImageURL[3],
                TalentImageURL[4],
                TalentImageURL[5],
                HttpUtility.HtmlEncode(TalentName[0]),
                HttpUtility.HtmlEncode(TalentName[1]),
                HttpUtility.HtmlEncode(TalentName[2]),
                HttpUtility.HtmlEncode(TalentName[3]),
                HttpUtility.HtmlEncode(TalentName[4]),
                HttpUtility.HtmlEncode(TalentName[5]))
            : string.Format(
                TalentBuildCardHTMLTemplate,
                WinPercent.ToString("P1"),
                GamesPlayed,
                HttpUtility.HtmlEncode(TalentNameDescription[0]),
                HttpUtility.HtmlEncode(TalentNameDescription[1]),
                HttpUtility.HtmlEncode(TalentNameDescription[2]),
                HttpUtility.HtmlEncode(TalentNameDescription[3]),
                HttpUtility.HtmlEncode(TalentNameDescription[4]),
                HttpUtility.HtmlEncode(TalentNameDescription[5]),
                HttpUtility.HtmlEncode(TalentNameDescription[6]),
                TalentImageURL[0],
                TalentImageURL[1],
                TalentImageURL[2],
                TalentImageURL[3],
                TalentImageURL[4],
                TalentImageURL[5],
                TalentImageURL[6],
                HttpUtility.HtmlEncode(TalentName[0]),
                HttpUtility.HtmlEncode(TalentName[1]),
                HttpUtility.HtmlEncode(TalentName[2]),
                HttpUtility.HtmlEncode(TalentName[3]),
                HttpUtility.HtmlEncode(TalentName[4]),
                HttpUtility.HtmlEncode(TalentName[5]),
                HttpUtility.HtmlEncode(TalentName[6]));
    }
}