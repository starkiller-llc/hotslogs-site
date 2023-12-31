﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heroes.DataAccessLayer.Models
{
    [Table("replay_dups")]
    public partial class ReplayDup
    {
        [Key]
        [Column("ReplayID", TypeName = "int(11)")]
        public int ReplayId { get; set; }
        [Column(TypeName = "int(11)")]
        public int ReplayBuild { get; set; }
        [Column(TypeName = "int(11)")]
        public int GameMode { get; set; }
        [Column("MapID", TypeName = "int(11)")]
        public int MapId { get; set; }
        [Column(TypeName = "time")]
        public TimeSpan ReplayLength { get; set; }
        [Required]
        [MaxLength(16)]
        public byte[] ReplayHash { get; set; }
        [Column(TypeName = "timestamp")]
        public DateTime TimestampReplay { get; set; }
        [Column(TypeName = "timestamp")]
        public DateTime TimestampCreated { get; set; }
        [Column("HOTSAPIFingerprint", TypeName = "varchar(36)")]
        public string Hotsapifingerprint { get; set; }
    }
}