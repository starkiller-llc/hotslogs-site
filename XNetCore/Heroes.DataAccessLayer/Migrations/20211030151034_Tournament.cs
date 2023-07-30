using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Heroes.DataAccessLayer.Migrations
{
    public partial class Tournament : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tournament",
                columns: table => new
                {
                    TournamentId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TournamentName = table.Column<string>(nullable: true),
                    TournamentDescription = table.Column<string>(type: "text", nullable: true),
                    RegistrationDeadline = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    IsPublic = table.Column<int>(nullable: false),
                    MaxNumTeams = table.Column<int>(nullable: true),
                    EntryFee = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournament", x => x.TournamentId);
                });

            migrationBuilder.CreateTable(
                name: "tournament_match",
                columns: table => new
                {
                    MatchId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TournamentId = table.Column<int>(nullable: false),
                    ReplayId = table.Column<int>(nullable: true),
                    RoundNum = table.Column<int>(nullable: false),
                    MatchCreated = table.Column<DateTime>(nullable: false),
                    MatchDeadline = table.Column<DateTime>(nullable: false),
                    MatchTime = table.Column<DateTime>(nullable: true),
                    Team1Id = table.Column<int>(nullable: false),
                    Team2Id = table.Column<int>(nullable: false),
                    WinningTeamId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournament_match", x => x.MatchId);
                });

            migrationBuilder.CreateTable(
                name: "tournament_participant",
                columns: table => new
                {
                    ParticipantId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TournamentId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false),
                    Battletag = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournament_participant", x => x.ParticipantId);
                });

            migrationBuilder.CreateTable(
                name: "tournament_team",
                columns: table => new
                {
                    TeamId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TournamentId = table.Column<int>(nullable: false),
                    TeamName = table.Column<string>(nullable: true),
                    CaptainEmail = table.Column<string>(nullable: true),
                    IsPaid = table.Column<int>(nullable: false),
                    PaypalEmail = table.Column<string>(nullable: true),
                    RegistrationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tournament_team", x => x.TeamId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tournament");

            migrationBuilder.DropTable(
                name: "tournament_match");

            migrationBuilder.DropTable(
                name: "tournament_participant");

            migrationBuilder.DropTable(
                name: "tournament_team");
        }
    }
}
