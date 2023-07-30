﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heroes.DataAccessLayer.Models
{
    [Table("blogposts")]
    public partial class BlogPost
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; }
        [Column(TypeName = "timestamp")]
        public DateTime CreateDate { get; set; }
        [Column(TypeName = "timestamp")]
        public DateTime? ExpireDate { get; set; }
        [Required]
        [Column(TypeName = "varchar(45)")]
        public string Tags { get; set; }
        public int Priority { get; set; }
    }
}