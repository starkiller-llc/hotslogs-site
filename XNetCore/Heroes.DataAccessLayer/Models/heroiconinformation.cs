﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heroes.DataAccessLayer.Models
{
    [Table("heroiconinformation")]
    public partial class HeroIconInformation
    {
        [Key]
        [Column("pkid", TypeName = "int(11)")]
        public int Pkid { get; set; }
        [Required]
        [Column("name", TypeName = "varchar(45)")]
        public string Name { get; set; }
        [Required]
        [Column("icon", TypeName = "varchar(255)")]
        public string Icon { get; set; }
    }
}