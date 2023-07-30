using Heroes.DataAccessLayer.CustomModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace Heroes.DataAccessLayer.Data;

public partial class HeroesdataContext
{
    public static HeroesdataContext Create(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<HeroesdataContext>();
    }

    public virtual DbSet<QueryContainerIsValidReplay> QueryContainerIsValidReplays { get; set; }
    public virtual DbSet<ReplayCharacterDetails> ReplayCharacterDetailsEnumerable { get; set; }
    public virtual DbSet<ReplayCharacterTalentCustom> ReplayCharacterTalentsCustom { get; set; }
    public virtual DbSet<PlayerMatchCustom> PlayerMatchCustoms { get; set; }
    public virtual DbSet<PlayerMatchCustom2> PlayerMatchCustoms2 { get; set; }
    public virtual DbSet<TeamProfileCustom> TeamProfileCustoms { get; set; }
    public virtual DbSet<TeamProfileReplayCharacterCustom> TeamProfileReplayCharacterCustoms { get; set; }
    public virtual DbSet<UpgradeCustom> UpgradeCustoms { get; set; }
    public virtual DbSet<TeamProfilePlayerCustom> TeamProfilePlayerCustoms { get; set; }
    public virtual DbSet<PlayerRelationshipCustom> PlayerRelationshipCustoms { get; set; }
    public virtual DbSet<TeamProfileReplayCustom> TeamProfileReplayCustoms { get; set; }
    public virtual DbSet<ProfileMapStatCustom> ProfileMapStatCustoms { get; set; }
    public virtual DbSet<ProfilePlayerStatCustom> ProfilePlayerStatCustoms { get; set; }
    public virtual DbSet<ProfileGameTimeWinRateCustom> ProfileGameTimeWinRateCustoms { get; set; }
    public virtual DbSet<ProfilePlayerRelationshipCustom> ProfilePlayerRelationshipCustoms { get; set; }
    public virtual DbSet<ProfileCharacterStatCustom> ProfileCharacterStatCustoms { get; set; }
    public virtual DbSet<PlayerSearchResultCustom> PlayerSearchResultCustoms { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder builder)
    {
        builder.Entity<QueryContainerIsValidReplay>().HasNoKey();
        builder.Entity<QueryContainerIsValidReplay>().Property(r => r.PlayerIDsStr).HasColumnName("PlayerIDs");
        builder.Entity<QueryContainerIsValidReplay>().Ignore(r => r.PlayerIDs);

        builder.Entity<ReplayCharacterDetails>().HasNoKey();
        //builder.Entity<ReplayCharacterDetails>().OwnsOne(r => r.ReplayCharacterScoreResult);
        builder.Entity<ReplayCharacterDetails>().Property(r => r.PlayerName).HasColumnName("Name");
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.MatchAwards);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.Character);
        builder.Entity<ReplayCharacterDetails>()
            .HasOne(r => r.ReplayCharacterScoreResult)
            .WithOne()
            .HasForeignKey<ReplayCharacterDetails>(
                r => new
                {
                    r.ReplayID,
                    r.PlayerID,
                })
            .IsRequired(false);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentImageURL01);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentImageURL04);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentImageURL07);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentImageURL10);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentImageURL13);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentImageURL16);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentImageURL20);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentNameDescription01);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentNameDescription04);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentNameDescription07);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentNameDescription10);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentNameDescription13);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentNameDescription16);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentNameDescription20);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentName01);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentName04);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentName07);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentName10);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentName13);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentName16);
        builder.Entity<ReplayCharacterDetails>().Ignore(r => r.TalentName20);

        builder.Entity<ReplayCharacterTalentCustom>().HasNoKey();

        builder.Entity<PlayerMatchCustom>().HasNoKey();

        builder.Entity<PlayerMatchCustom2>().HasNoKey();

        builder.Entity<TeamProfileCustom>().HasNoKey();

        builder.Entity<TeamProfileReplayCharacterCustom>().HasNoKey();

        builder.Entity<UpgradeCustom>().HasNoKey();

        builder.Entity<TeamProfilePlayerCustom>().HasNoKey();

        builder.Entity<PlayerRelationshipCustom>().HasNoKey();

        builder.Entity<TeamProfileReplayCustom>().HasNoKey();

        builder.Entity<ProfileMapStatCustom>().HasNoKey();

        builder.Entity<ProfilePlayerStatCustom>().HasNoKey();

        builder.Entity<ProfileGameTimeWinRateCustom>().HasNoKey();

        builder.Entity<ProfilePlayerRelationshipCustom>().HasNoKey();

        builder.Entity<ProfileCharacterStatCustom>().HasNoKey();

        builder.Entity<PlayerSearchResultCustom>().HasNoKey();
    }
}
