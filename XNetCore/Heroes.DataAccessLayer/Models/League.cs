﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heroes.DataAccessLayer.Models
{
    [Table("league")]
    public partial class League
    {
        public League()
        {
            LeaderboardRankings = new HashSet<LeaderboardRanking>();
        }

        [Key]
        [Column("LeagueID", TypeName = "int(11)")]
        public int LeagueId { get; set; }
        [Required]
        [Column(TypeName = "varchar(50)")]
        public string LeagueName { get; set; }
        [Column(TypeName = "int(11)")]
        public int RequiredGames { get; set; }

        [InverseProperty(nameof(LeaderboardRanking.League))]
        public virtual ICollection<LeaderboardRanking> LeaderboardRankings { get; set; }
    }
}