﻿using Heroes.DataAccessLayer.Models;
using System;
using System.Linq;

namespace Heroes.DataAccessLayer.CustomModels;

public class ReplayCharacterDetails
{
    private int[] _matchAwards;
    public int BattleNetId { get; set; }
    public int BattleNetRegionId { get; set; }
    public int BattleNetSubId { get; set; }
    public int CharacterID { get; set; }
    public string Character { get; set; }
    public int CharacterLevel { get; set; }
    public bool IsLeaderboardOptOut { get; set; }
    public bool IsWinner { get; set; }

    public string MatchAwardTypes { get; set; }

    public int[] MatchAwards => _matchAwards ??= MatchAwardTypes?.Split(',').Select(int.Parse).ToArray() ?? Array.Empty<int>();

    public int? MMRBefore { get; set; }
    public int? MMRChange { get; set; }
    public int PlayerID { get; set; }
    public string PlayerName { get; set; }

    public ReplayCharacterScoreResult ReplayCharacterScoreResult { get; set; }
    public int ReplayID { get; set; }
    public int Reputation { get; set; }

    public string TalentImageURL01 { get; set; }
    public string TalentImageURL04 { get; set; }
    public string TalentImageURL07 { get; set; }
    public string TalentImageURL10 { get; set; }
    public string TalentImageURL13 { get; set; }
    public string TalentImageURL16 { get; set; }
    public string TalentImageURL20 { get; set; }

    public string TalentNameDescription01 { get; set; }
    public string TalentNameDescription04 { get; set; }
    public string TalentNameDescription07 { get; set; }
    public string TalentNameDescription10 { get; set; }
    public string TalentNameDescription13 { get; set; }
    public string TalentNameDescription16 { get; set; }
    public string TalentNameDescription20 { get; set; }
    public string TalentName01 { get; set; }
    public string TalentName04 { get; set; }
    public string TalentName07 { get; set; }
    public string TalentName10 { get; set; }
    public string TalentName13 { get; set; }
    public string TalentName16 { get; set; }
    public string TalentName20 { get; set; }
}
