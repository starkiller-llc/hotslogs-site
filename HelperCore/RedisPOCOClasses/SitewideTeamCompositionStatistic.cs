using System;

namespace HelperCore.RedisPOCOClasses;

[Serializable]
public class SitewideTeamCompositionStatistic
{
    public int GamesPlayed { get; set; }
    public decimal WinPercent { get; set; }

    public string CharacterNamesCSV { get; set; }

    public string CharacterName1 { get; set; }
    public string CharacterName2 { get; set; }
    public string CharacterName3 { get; set; }
    public string CharacterName4 { get; set; }
    public string CharacterName5 { get; set; }

    public string CharacterImageURL1 { get; set; }
    public string CharacterImageURL2 { get; set; }
    public string CharacterImageURL3 { get; set; }
    public string CharacterImageURL4 { get; set; }
    public string CharacterImageURL5 { get; set; }
}