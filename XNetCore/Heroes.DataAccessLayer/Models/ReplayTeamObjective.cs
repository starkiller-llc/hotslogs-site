﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heroes.DataAccessLayer.Models
{
    [Table("replayteamobjective")]
    public partial class ReplayTeamObjective
    {
        [Key]
        [Column("ReplayID", TypeName = "int(11)")]
        public int ReplayId { get; set; }
        [Key]
        [Column(TypeName = "bit(1)")]
        public ulong IsWinner { get; set; }
        [Key]
        [Column(TypeName = "int(11)")]
        public int TeamObjectiveType { get; set; }
        [Key]
        [Column(TypeName = "time")]
        public TimeSpan TimeSpan { get; set; }
        [Column("PlayerID", TypeName = "int(11)")]
        public int? PlayerId { get; set; }
        [Column(TypeName = "int(11)")]
        public int Value { get; set; }

        [ForeignKey(nameof(PlayerId))]
        [InverseProperty("ReplayTeamObjectives")]
        public virtual Player Player { get; set; }
        [ForeignKey(nameof(ReplayId))]
        [InverseProperty("ReplayTeamObjectives")]
        public virtual Replay Replay { get; set; }
    }
}