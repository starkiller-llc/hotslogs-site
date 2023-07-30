// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Heroes.DataAccessLayer.Models
{
    [Table("tournament")]
    public partial class Tournament
    {
        [Key]
        public int TournamentId { get; set; }
        public string TournamentName { get; set; }
        [Column(TypeName = "text")]
        public string TournamentDescription { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public DateTime? EndDate { get; set; }
        public int IsPublic { get; set; }
        public int? MaxNumTeams { get; set; }
        public decimal EntryFee { get; set; }
    }
}
