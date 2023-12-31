﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heroes.DataAccessLayer.Models
{
    [Table("hotsapireplays")]
    public partial class HotsApiReplay
    {
        [Key]
        [Column("id", TypeName = "int(10) unsigned")]
        public uint Id { get; set; }
        [Column("parsed_id", TypeName = "int(10) unsigned")]
        public uint? ParsedId { get; set; }
        [Column("created_at", TypeName = "timestamp")]
        public DateTime? CreatedAt { get; set; }
        [Column("updated_at", TypeName = "timestamp")]
        public DateTime? UpdatedAt { get; set; }
        [Required]
        [Column("filename", TypeName = "varchar(36)")]
        public string Filename { get; set; }
        [Column("size", TypeName = "int(10) unsigned")]
        public uint Size { get; set; }
        [Column("game_type", TypeName = "enum('QuickMatch','UnrankedDraft','HeroLeague','TeamLeague','Brawl','StormLeague')")]
        public string GameType { get; set; }
        [Column("game_date", TypeName = "datetime")]
        public DateTime? GameDate { get; set; }
        [Column("game_length", TypeName = "smallint(5) unsigned")]
        public ushort? GameLength { get; set; }
        [Column("game_map_id", TypeName = "int(10) unsigned")]
        public uint? GameMapId { get; set; }
        [Column("game_version", TypeName = "varchar(32)")]
        public string GameVersion { get; set; }
        [Column("region", TypeName = "tinyint(3) unsigned")]
        public byte? Region { get; set; }
        [Required]
        [Column("fingerprint", TypeName = "varchar(36)")]
        public string Fingerprint { get; set; }
        [Column("processed", TypeName = "tinyint(4)")]
        public sbyte Processed { get; set; }
        [Column("deleted")]
        public bool Deleted { get; set; }
    }
}