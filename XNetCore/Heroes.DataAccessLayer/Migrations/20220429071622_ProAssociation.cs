using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Heroes.DataAccessLayer.Migrations
{
    public partial class ProAssociation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "event_team",
                columns: table => new
                {
                    TeamId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EventID = table.Column<int>(type: "int(11)", nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_team", x => x.TeamId);
                    table.ForeignKey(
                        name: "FK_event_team_event_EventID",
                        column: x => x.EventID,
                        principalTable: "event",
                        principalColumn: "EventID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "replaycharacter_pro_association",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false),
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false),
                    TeamId = table.Column<int>(nullable: true),
                    ProName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.ReplayID, x.PlayerID });
                    table.ForeignKey(
                        name: "FK_replaycharacter_pro_association_player_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_replaycharacter_pro_association_replay_ReplayID",
                        column: x => x.ReplayID,
                        principalTable: "replay",
                        principalColumn: "ReplayID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_replaycharacter_pro_association_event_team_TeamId",
                        column: x => x.TeamId,
                        principalTable: "event_team",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_replaycharacter_pro_association_replaycharacter_ReplayID_Pla~",
                        columns: x => new { x.ReplayID, x.PlayerID },
                        principalTable: "replaycharacter",
                        principalColumns: new[] { "ReplayID", "PlayerID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_event_team_EventID",
                table: "event_team",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerID",
                table: "replaycharacter_pro_association",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayID",
                table: "replaycharacter_pro_association",
                column: "ReplayID");

            migrationBuilder.CreateIndex(
                name: "IX_replaycharacter_pro_association_TeamId",
                table: "replaycharacter_pro_association",
                column: "TeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "replaycharacter_pro_association");

            migrationBuilder.DropTable(
                name: "event_team");
        }
    }
}
