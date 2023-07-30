﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heroes.DataAccessLayer.Models
{
    [Table("votes")]
    public partial class Vote
    {
        [Key]
        public int Id { get; set; }
        public int VotingPlayerId { get; set; }
        public int TargetPlayerId { get; set; }
        public int TargetReplayId { get; set; }
        [Column(TypeName = "bit(1)")]
        public ulong? Up { get; set; }
    }
}