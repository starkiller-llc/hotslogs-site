using Microsoft.EntityFrameworkCore.Migrations;

namespace Heroes.DataAccessLayer.Migrations
{
    public partial class TeamLogoUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_replaycharacter_pro_association_event_team_TeamId",
                table: "replaycharacter_pro_association");

            migrationBuilder.AlterColumn<int>(
                name: "TeamId",
                table: "replaycharacter_pro_association",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "event_team",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_replaycharacter_pro_association_event_team_TeamId",
                table: "replaycharacter_pro_association",
                column: "TeamId",
                principalTable: "event_team",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_replaycharacter_pro_association_event_team_TeamId",
                table: "replaycharacter_pro_association");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "event_team");

            migrationBuilder.AlterColumn<int>(
                name: "TeamId",
                table: "replaycharacter_pro_association",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_replaycharacter_pro_association_event_team_TeamId",
                table: "replaycharacter_pro_association",
                column: "TeamId",
                principalTable: "event_team",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
