﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heroes.DataAccessLayer.Models
{
    [Table("replay_playertalentbuilds")]
    public partial class ReplayPlayerTalentBuild
    {
        [Key]
        [Column("replayid")]
        public int Replayid { get; set; }
        [Key]
        [Column("playerid")]
        public int Playerid { get; set; }
        [Required]
        [Column("talentselection", TypeName = "varchar(20)")]
        public string Talentselection { get; set; }
    }
}