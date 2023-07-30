// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heroes.DataAccessLayer.Models
{
    [Table("tournament_match")]
    public partial class TournamentMatchDB
    {
        [Key]
        public int MatchId { get; set; }
        public int TournamentId { get; set; }
        public int? ReplayId { get; set; }
        public int RoundNum { get; set; }
        public DateTime MatchCreated { get; set; }
        public DateTime MatchDeadline { get; set; }
        public DateTime? MatchTime { get; set; }
        public int Team1Id { get; set; }
        public int Team2Id { get; set; }
        public int? WinningTeamId { get; set; }
    }
}