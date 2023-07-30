using System;

namespace Heroes.DataAccessLayer.CustomModels;

public class TeamProfileReplayCharacterCustom
{
    public int ReplayID { get; set; }
    public int PlayerID { get; set; }
    public UInt64 IsAutoSelect { get; set; }
    public string Character { get; set; }
    public int CharacterLevel { get; set; }
    public UInt64 IsWinner { get; set; }
    public int? MMRBefore { get; set; }
    public int? MMRChange { get; set; }
    public int? Takedowns { get; set; }
    public int? SoloKills { get; set; }
    public int? Assists { get; set; }
    public int? Deaths { get; set; }
    public int? HeroDamage { get; set; }
    public int? SiegeDamage { get; set; }
    public int? StructureDamage { get; set; }
    public int? MinionDamage { get; set; }
    public int? CreepDamage { get; set; }
    public int? SummonDamage { get; set; }
    public TimeSpan? TimeCCdEnemyHeroes { get; set; }
    public int? Healing { get; set; }
    public int? SelfHealing { get; set; }
    public int? DamageTaken { get; set; }
    public int? ExperienceContribution { get; set; }
    public int? TownKills { get; set; }
    public TimeSpan? TimeSpentDead { get; set; }
    public int? MercCampCaptures { get; set; }
    public int? WatchTowerCaptures { get; set; }
    public int? MetaExperience { get; set; }
}
