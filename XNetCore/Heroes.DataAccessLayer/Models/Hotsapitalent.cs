﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heroes.DataAccessLayer.Models
{
    [Table("hotsapitalents")]
    public partial class HotsApiTalent
    {
        [Key]
        [Column("pkid", TypeName = "int(11)")]
        public int Pkid { get; set; }
        [Column(TypeName = "varchar(30)")]
        public string Hero { get; set; }
        [Column("TalentID", TypeName = "int(11)")]
        public int TalentId { get; set; }
        [Column(TypeName = "int(11)")]
        public int Sort { get; set; }
        [Column(TypeName = "int(11)")]
        public int Level { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string Title { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string Description { get; set; }
    }
}