using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Heroes.DataAccessLayer.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "amazonreplacementbucket",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Blob = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_amazonreplacementbucket", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "blogposts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    CreateDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpireDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Tags = table.Column<string>(type: "varchar(45)", nullable: false, defaultValueSql: "'@main@'")
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Priority = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blogposts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "buildnumbers",
                columns: table => new
                {
                    buildnumber = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    builddate = table.Column<DateTime>(type: "date", nullable: true),
                    version = table.Column<string>(type: "varchar(45)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.buildnumber);
                });

            migrationBuilder.CreateTable(
                name: "dataupdate",
                columns: table => new
                {
                    DataEvent = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    LastUpdated = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DataEvent);
                });

            migrationBuilder.CreateTable(
                name: "event",
                columns: table => new
                {
                    EventID = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EventIDParent = table.Column<int>(type: "int(11)", nullable: true),
                    EventName = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    EventOrder = table.Column<int>(type: "int(11)", nullable: false),
                    EventGamesPlayed = table.Column<int>(type: "int(11)", nullable: false),
                    IsEnabled = table.Column<ulong>(type: "bit(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event", x => x.EventID);
                    table.ForeignKey(
                        name: "FK_EventIDParent_EventID",
                        column: x => x.EventIDParent,
                        principalTable: "event",
                        principalColumn: "EventID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fingerprint_date",
                columns: table => new
                {
                    _20170827145118 = table.Column<DateTime>(name: "2017-08-27 14:51:18", type: "datetime", nullable: true),
                    _725ba4982728d326b6ac11129c55b212 = table.Column<string>(name: "725ba498-2728-d326-b6ac-11129c55b212", type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci")
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "heroiconinformation",
                columns: table => new
                {
                    pkid = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(45)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    icon = table.Column<string>(type: "varchar(255)", nullable: false, defaultValueSql: "'~/Images/Heroes/Portraits/AutoSelect.png'")
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.pkid);
                });

            migrationBuilder.CreateTable(
                name: "herotalentinformation",
                columns: table => new
                {
                    Character = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    ReplayBuildFirst = table.Column<int>(type: "int(11)", nullable: false),
                    TalentID = table.Column<int>(type: "int(11)", nullable: false),
                    ReplayBuildLast = table.Column<int>(type: "int(11)", nullable: false),
                    TalentTier = table.Column<int>(type: "int(11)", nullable: false),
                    TalentName = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    TalentDescription = table.Column<string>(type: "varchar(1000)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.Character, x.ReplayBuildFirst, x.TalentID });
                });

            migrationBuilder.CreateTable(
                name: "hotsapireplays",
                columns: table => new
                {
                    id = table.Column<uint>(type: "int(10) unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    parsed_id = table.Column<uint>(type: "int(10) unsigned", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    filename = table.Column<string>(type: "varchar(36)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                        .Annotation("MySql:Collation", "utf8mb4_0900_ai_ci"),
                    size = table.Column<uint>(type: "int(10) unsigned", nullable: false),
                    game_type = table.Column<string>(type: "enum('QuickMatch','UnrankedDraft','HeroLeague','TeamLeague','Brawl','StormLeague')", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                        .Annotation("MySql:Collation", "utf8mb4_0900_ai_ci"),
                    game_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    game_length = table.Column<ushort>(type: "smallint(5) unsigned", nullable: true),
                    game_map_id = table.Column<uint>(type: "int(10) unsigned", nullable: true),
                    game_version = table.Column<string>(type: "varchar(32)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                        .Annotation("MySql:Collation", "utf8mb4_0900_ai_ci"),
                    region = table.Column<byte>(type: "tinyint(3) unsigned", nullable: true),
                    fingerprint = table.Column<string>(type: "varchar(36)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                        .Annotation("MySql:Collation", "utf8mb4_0900_ai_ci"),
                    processed = table.Column<sbyte>(type: "tinyint(4)", nullable: false),
                    deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotsapireplays", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hotsapitalents",
                columns: table => new
                {
                    pkid = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Hero = table.Column<string>(type: "varchar(30)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    TalentID = table.Column<int>(type: "int(11)", nullable: false),
                    Sort = table.Column<int>(type: "int(11)", nullable: false),
                    Level = table.Column<int>(type: "int(11)", nullable: false, defaultValueSql: "'1'"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Title = table.Column<string>(type: "varchar(100)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.pkid);
                });

            migrationBuilder.CreateTable(
                name: "league",
                columns: table => new
                {
                    LeagueID = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LeagueName = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    RequiredGames = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_league", x => x.LeagueID);
                });

            migrationBuilder.CreateTable(
                name: "localizationalias",
                columns: table => new
                {
                    IdentifierID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(nullable: false),
                    PrimaryName = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    AttributeName = table.Column<string>(type: "varchar(40)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Group = table.Column<string>(type: "varchar(50)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    SubGroup = table.Column<string>(type: "varchar(50)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    AliasesCSV = table.Column<string>(type: "varchar(2000)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    NewGroup = table.Column<string>(type: "varchar(50)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.IdentifierID);
                });

            migrationBuilder.CreateTable(
                name: "missingtalents",
                columns: table => new
                {
                    Character = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Build = table.Column<int>(type: "int(11)", nullable: false),
                    TalentID = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.Character, x.Build, x.TalentID });
                });

            migrationBuilder.CreateTable(
                name: "mmrrecalc",
                columns: table => new
                {
                    BattleNetRegionID = table.Column<int>(nullable: false),
                    GameMode = table.Column<int>(nullable: false),
                    TipOld = table.Column<DateTime>(type: "timestamp", nullable: true),
                    TipRecent = table.Column<DateTime>(type: "timestamp", nullable: true),
                    TipManual = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.BattleNetRegionID, x.GameMode });
                });

            migrationBuilder.CreateTable(
                name: "mountinformation",
                columns: table => new
                {
                    AttributeId = table.Column<string>(type: "varchar(4)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Description = table.Column<string>(type: "varchar(1000)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Franchise = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    ReleaseDate = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.AttributeId);
                });

            migrationBuilder.CreateTable(
                name: "player",
                columns: table => new
                {
                    PlayerID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BattleNetRegionId = table.Column<int>(nullable: false),
                    BattleNetSubId = table.Column<int>(nullable: false),
                    BattleNetId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                        .Annotation("MySql:Collation", "utf8mb4_0900_as_ci"),
                    BattleTag = table.Column<int>(nullable: true),
                    TimestampCreated = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "'2020-01-14 18:00:00'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player", x => x.PlayerID);
                });

            migrationBuilder.CreateTable(
                name: "playermmrreset",
                columns: table => new
                {
                    ResetDate = table.Column<DateTime>(type: "date", nullable: false),
                    Title = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    MMRMeanMultiplier = table.Column<double>(nullable: false),
                    MMRStandardDeviationGapMultiplier = table.Column<double>(nullable: false),
                    IsClampOutliers = table.Column<ulong>(type: "bit(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ResetDate);
                });

            migrationBuilder.CreateTable(
                name: "premiumpayment",
                columns: table => new
                {
                    TransactionID = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Email = table.Column<string>(type: "varchar(128)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    ItemTitle = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    PaymentAmountGross = table.Column<decimal>(type: "decimal(15,4)", nullable: false),
                    PaymentAmountFee = table.Column<decimal>(type: "decimal(15,4)", nullable: false),
                    TimestampPayment = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TransactionID);
                });

            migrationBuilder.CreateTable(
                name: "replay",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReplayBuild = table.Column<int>(type: "int(11)", nullable: false),
                    GameMode = table.Column<int>(type: "int(11)", nullable: false),
                    MapID = table.Column<int>(type: "int(11)", nullable: false),
                    ReplayLength = table.Column<TimeSpan>(type: "time", nullable: false),
                    ReplayHash = table.Column<byte[]>(fixedLength: true, maxLength: 16, nullable: false),
                    TimestampReplay = table.Column<DateTime>(type: "timestamp", nullable: false),
                    TimestampCreated = table.Column<DateTime>(type: "timestamp", nullable: false),
                    HOTSAPIFingerprint = table.Column<string>(type: "varchar(36)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_replay", x => x.ReplayID);
                });

            migrationBuilder.CreateTable(
                name: "replay_dups",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReplayBuild = table.Column<int>(type: "int(11)", nullable: false),
                    GameMode = table.Column<int>(type: "int(11)", nullable: false),
                    MapID = table.Column<int>(type: "int(11)", nullable: false),
                    ReplayLength = table.Column<TimeSpan>(type: "time", nullable: false),
                    ReplayHash = table.Column<byte[]>(fixedLength: true, maxLength: 16, nullable: false),
                    TimestampReplay = table.Column<DateTime>(type: "timestamp", nullable: false),
                    TimestampCreated = table.Column<DateTime>(type: "timestamp", nullable: false),
                    HOTSAPIFingerprint = table.Column<string>(type: "varchar(36)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ReplayID);
                });

            migrationBuilder.CreateTable(
                name: "replay_dups2",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DupOfReplayID = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ReplayID);
                });

            migrationBuilder.CreateTable(
                name: "replay_mirror",
                columns: table => new
                {
                    ReplayID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ReplayID);
                });

            migrationBuilder.CreateTable(
                name: "replay_notalents",
                columns: table => new
                {
                    ReplayID = table.Column<int>(nullable: false),
                    ReplayBuild = table.Column<int>(nullable: false),
                    GameMode = table.Column<int>(nullable: false),
                    MapID = table.Column<int>(nullable: false),
                    ReplayLength = table.Column<TimeSpan>(type: "time", nullable: false),
                    ReplayHash = table.Column<byte[]>(fixedLength: true, maxLength: 16, nullable: false),
                    TimestampReplay = table.Column<DateTime>(type: "timestamp", nullable: false),
                    TimestampCreated = table.Column<DateTime>(type: "timestamp", nullable: false),
                    HOTSAPIFingerprint = table.Column<string>(type: "varchar(36)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci")
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "replay_playertalentbuilds",
                columns: table => new
                {
                    replayid = table.Column<int>(nullable: false),
                    playerid = table.Column<int>(nullable: false),
                    talentselection = table.Column<string>(type: "varchar(20)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                        .Annotation("MySql:Collation", "utf8mb4_0900_ai_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.replayid, x.playerid });
                });

            migrationBuilder.CreateTable(
                name: "talentimagemapping",
                columns: table => new
                {
                    TalentName = table.Column<string>(type: "varchar(45)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    HeroName = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    TalentImage = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Character = table.Column<string>(type: "varchar(45)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.TalentName, x.HeroName });
                });

            migrationBuilder.CreateTable(
                name: "unknowndata",
                columns: table => new
                {
                    UnknownData = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_bin")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.UnknownData);
                });

            migrationBuilder.CreateTable(
                name: "votes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VotingPlayerId = table.Column<int>(nullable: false),
                    TargetPlayerId = table.Column<int>(nullable: false),
                    TargetReplayId = table.Column<int>(nullable: false),
                    Up = table.Column<ulong>(type: "bit(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_votes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "zamuser",
                columns: table => new
                {
                    ID = table.Column<string>(type: "varchar(36)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Email = table.Column<string>(type: "varchar(128)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Username = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    PremiumExpiration = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsHotslogsPremiumConverted = table.Column<int>(type: "int(11)", nullable: true),
                    TimestampCreated = table.Column<DateTime>(type: "datetime", nullable: false),
                    TimestampLastUpdated = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zamuser", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "groupfinderlisting",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false),
                    GroupFinderListingTypeID = table.Column<int>(type: "int(11)", nullable: false),
                    Information = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    MMRSearchRadius = table.Column<int>(type: "int(11)", nullable: false),
                    TimestampExpiration = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PlayerID);
                    table.ForeignKey(
                        name: "FK_GroupFinderListing_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leaderboardoptout",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PlayerID);
                    table.ForeignKey(
                        name: "FK_LeaderboardOptOut_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leaderboardranking",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false),
                    GameMode = table.Column<int>(type: "int(11)", nullable: false),
                    CurrentMMR = table.Column<int>(type: "int(11)", nullable: false),
                    LeagueID = table.Column<int>(type: "int(11)", nullable: true),
                    LeagueRank = table.Column<int>(type: "int(11)", nullable: true),
                    IsEligibleForLeaderboard = table.Column<ulong>(type: "bit(1)", nullable: false, defaultValueSql: "b'0'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.PlayerID, x.GameMode });
                    table.ForeignKey(
                        name: "FK_LeaderboardRanking_League",
                        column: x => x.LeagueID,
                        principalTable: "league",
                        principalColumn: "LeagueID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaderboardRanking_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "net48_users",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    email = table.Column<string>(type: "varchar(45)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    username = table.Column<string>(type: "varchar(45)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    password = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    acceptedTOS = table.Column<ulong>(type: "bit(1)", nullable: false, defaultValueSql: "b'0'"),
                    userGUID = table.Column<string>(type: "varchar(45)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    premium = table.Column<int>(nullable: false),
                    resettoken = table.Column<string>(type: "varchar(255)", nullable: true, defaultValueSql: "''")
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    tokengenerated = table.Column<DateTime>(type: "datetime", nullable: true),
                    admin = table.Column<int>(nullable: true, defaultValueSql: "'0'"),
                    subscriptionid = table.Column<string>(type: "varchar(45)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    applicationId = table.Column<int>(nullable: false),
                    isAnonymous = table.Column<bool>(nullable: false, defaultValueSql: "'1'"),
                    lastActivityDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsBattleNetOAuthAuthorized = table.Column<ulong>(type: "bit(1)", nullable: false, defaultValueSql: "b'0'"),
                    IsGroupFinderAuthorized3 = table.Column<ulong>(type: "bit(1)", nullable: false, defaultValueSql: "b'1'"),
                    IsGroupFinderAuthorized4 = table.Column<ulong>(type: "bit(1)", nullable: false, defaultValueSql: "b'1'"),
                    IsGroupFinderAuthorized5 = table.Column<ulong>(type: "bit(1)", nullable: false, defaultValueSql: "b'1'"),
                    playerID = table.Column<int>(nullable: true),
                    LastLoginDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastPasswordChangedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsLockedOut = table.Column<bool>(nullable: true),
                    LastLockedOutDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    FailedPasswordAttemptCount = table.Column<uint>(nullable: true),
                    FailedPasswordAttemptWindowStart = table.Column<DateTime>(type: "datetime", nullable: true),
                    FailedPasswordAnswerAttemptCount = table.Column<uint>(nullable: true),
                    FailedPasswordAnswerAttemptWindowStart = table.Column<DateTime>(type: "datetime", nullable: true),
                    expiration = table.Column<DateTime>(type: "datetime", nullable: true),
                    PremiumSupporterSince = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_net48_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_player",
                        column: x => x.playerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "playeraggregate",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false),
                    GameMode = table.Column<int>(type: "int(11)", nullable: false),
                    GamesPlayedTotal = table.Column<int>(type: "int(11)", nullable: false),
                    GamesPlayedWithMMR = table.Column<int>(type: "int(11)", nullable: false),
                    GamesPlayedRecently = table.Column<int>(type: "int(11)", nullable: false),
                    FavoriteCharacter = table.Column<int>(type: "int(11)", nullable: false),
                    TimestampLastUpdated = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.PlayerID, x.GameMode });
                    table.ForeignKey(
                        name: "FK_PlayerAggregate_LocalizationAlias",
                        column: x => x.FavoriteCharacter,
                        principalTable: "localizationalias",
                        principalColumn: "IdentifierID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerAggregate_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "playeralt",
                columns: table => new
                {
                    PlayerIDAlt = table.Column<int>(type: "int(11)", nullable: false),
                    PlayerIDMain = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PlayerIDAlt);
                    table.ForeignKey(
                        name: "FK_PlayerIDAlt",
                        column: x => x.PlayerIDAlt,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerIDMain",
                        column: x => x.PlayerIDMain,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "playerbanned",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PlayerID);
                    table.ForeignKey(
                        name: "FK_PlayerBanned_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "playerbannedleaderboard",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PlayerID);
                    table.ForeignKey(
                        name: "FK_PlayerBannedLeaderboard_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "playerdisablenamechange",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PlayerID);
                    table.ForeignKey(
                        name: "FK_PlayerDisableNameChange_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "playermmrmilestonev3",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false),
                    GameMode = table.Column<int>(type: "int(11)", nullable: false),
                    MilestoneDate = table.Column<DateTime>(type: "date", nullable: false),
                    MMRMean = table.Column<double>(nullable: false),
                    MMRStandardDeviation = table.Column<double>(nullable: false),
                    MMRRating = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.PlayerID, x.MilestoneDate, x.GameMode });
                    table.ForeignKey(
                        name: "FK_PlayerMMRMilestoneV3_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reputation",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false),
                    Reputation = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_Players",
                        column: x => x.PlayerId,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "replaycharacter",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false),
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false),
                    IsAutoSelect = table.Column<ulong>(type: "bit(1)", nullable: false),
                    CharacterID = table.Column<int>(type: "int(11)", nullable: false),
                    CharacterLevel = table.Column<int>(type: "int(11)", nullable: false),
                    IsWinner = table.Column<ulong>(type: "bit(1)", nullable: false),
                    MMRBefore = table.Column<int>(type: "int(11)", nullable: true),
                    MMRChange = table.Column<int>(type: "int(11)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.ReplayID, x.PlayerID });
                    table.ForeignKey(
                        name: "FK_ReplayCharacter_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReplayCharacter_Replay",
                        column: x => x.ReplayID,
                        principalTable: "replay",
                        principalColumn: "ReplayID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "replayperiodicxpbreakdown",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false),
                    IsWinner = table.Column<ulong>(type: "bit(1)", nullable: false),
                    GameTimeMinute = table.Column<int>(type: "int(11)", nullable: false),
                    MinionXP = table.Column<int>(type: "int(11)", nullable: false),
                    CreepXP = table.Column<int>(type: "int(11)", nullable: false),
                    StructureXP = table.Column<int>(type: "int(11)", nullable: false),
                    HeroXP = table.Column<int>(type: "int(11)", nullable: false),
                    TrickleXP = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.ReplayID, x.IsWinner, x.GameTimeMinute });
                    table.ForeignKey(
                        name: "FK_ReplayPeriodicXPBreakdown_Replay",
                        column: x => x.ReplayID,
                        principalTable: "replay",
                        principalColumn: "ReplayID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "replayshare",
                columns: table => new
                {
                    ReplayShareID = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false),
                    PlayerIDSharedBy = table.Column<int>(type: "int(11)", nullable: false),
                    AlteredReplayFileName = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Title = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    Description = table.Column<string>(type: "mediumtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    UpvoteScore = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_replayshare", x => x.ReplayShareID);
                    table.ForeignKey(
                        name: "FK_ReplayShare_Player",
                        column: x => x.PlayerIDSharedBy,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReplayShare_Replay",
                        column: x => x.ReplayID,
                        principalTable: "replay",
                        principalColumn: "ReplayID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "replayteamheroban",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false),
                    CharacterID = table.Column<int>(type: "int(11)", nullable: false),
                    IsWinner = table.Column<ulong>(type: "bit(1)", nullable: false),
                    BanPhase = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.ReplayID, x.CharacterID });
                    table.ForeignKey(
                        name: "FK_ReplayTeamHeroBan_Replay",
                        column: x => x.ReplayID,
                        principalTable: "replay",
                        principalColumn: "ReplayID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "replayteamobjective",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false),
                    IsWinner = table.Column<ulong>(type: "bit(1)", nullable: false),
                    TeamObjectiveType = table.Column<int>(type: "int(11)", nullable: false),
                    TimeSpan = table.Column<TimeSpan>(type: "time", nullable: false),
                    PlayerID = table.Column<int>(type: "int(11)", nullable: true),
                    Value = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.ReplayID, x.IsWinner, x.TeamObjectiveType, x.TimeSpan });
                    table.ForeignKey(
                        name: "FK_this_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_this_Replay",
                        column: x => x.ReplayID,
                        principalTable: "replay",
                        principalColumn: "ReplayID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "logerror",
                columns: table => new
                {
                    LogErrorID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AbsoluteUri = table.Column<string>(type: "varchar(500)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    UserAgent = table.Column<string>(type: "varchar(500)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    UserHostAddress = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    UserID = table.Column<int>(nullable: true),
                    ErrorMessage = table.Column<string>(type: "mediumtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_general_ci"),
                    DateTimeErrorOccurred = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "'2020-01-20 13:27:33'"),
                    Referer = table.Column<string>(type: "varchar(500)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8")
                        .Annotation("MySql:Collation", "utf8_bin")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logerror", x => x.LogErrorID);
                    table.ForeignKey(
                        name: "FK_LogError_my_aspnet_users",
                        column: x => x.UserID,
                        principalTable: "net48_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "replaycharacterdraftorder",
                columns: table => new
                {
                    ReplayID = table.Column<int>(nullable: false),
                    PlayerID = table.Column<int>(nullable: false),
                    DraftOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.ReplayID, x.PlayerID });
                    table.ForeignKey(
                        name: "FK_ReplayCharacterDraftOrder_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReplayCharacterDraftOrder_Replay",
                        column: x => x.ReplayID,
                        principalTable: "replay",
                        principalColumn: "ReplayID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReplayCharacterDraftOrder_ReplayCharacter",
                        columns: x => new { x.ReplayID, x.PlayerID },
                        principalTable: "replaycharacter",
                        principalColumns: new[] { "ReplayID", "PlayerID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "replaycharactermatchaward",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false),
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false),
                    MatchAwardType = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.ReplayID, x.PlayerID, x.MatchAwardType });
                    table.ForeignKey(
                        name: "FK_ReplayCharacterMatchAward_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReplayCharacterMatchAward_Replay",
                        column: x => x.ReplayID,
                        principalTable: "replay",
                        principalColumn: "ReplayID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReplayCharacterMatchAward_ReplayCharacter",
                        columns: x => new { x.ReplayID, x.PlayerID },
                        principalTable: "replaycharacter",
                        principalColumns: new[] { "ReplayID", "PlayerID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "replaycharacterscoreresult",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false),
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false),
                    Level = table.Column<int>(type: "int(11)", nullable: true),
                    Takedowns = table.Column<int>(type: "int(11)", nullable: false),
                    SoloKills = table.Column<int>(type: "int(11)", nullable: false),
                    Assists = table.Column<int>(type: "int(11)", nullable: false),
                    Deaths = table.Column<int>(type: "int(11)", nullable: false),
                    HighestKillStreak = table.Column<int>(type: "int(11)", nullable: true),
                    HeroDamage = table.Column<int>(type: "int(11)", nullable: false),
                    SiegeDamage = table.Column<int>(type: "int(11)", nullable: false),
                    StructureDamage = table.Column<int>(type: "int(11)", nullable: false),
                    MinionDamage = table.Column<int>(type: "int(11)", nullable: false),
                    CreepDamage = table.Column<int>(type: "int(11)", nullable: false),
                    SummonDamage = table.Column<int>(type: "int(11)", nullable: false),
                    TimeCCdEnemyHeroes = table.Column<TimeSpan>(type: "time", nullable: true),
                    Healing = table.Column<int>(type: "int(11)", nullable: true),
                    SelfHealing = table.Column<int>(type: "int(11)", nullable: false),
                    DamageTaken = table.Column<int>(type: "int(11)", nullable: true),
                    ExperienceContribution = table.Column<int>(type: "int(11)", nullable: false),
                    TownKills = table.Column<int>(type: "int(11)", nullable: false),
                    TimeSpentDead = table.Column<TimeSpan>(type: "time", nullable: false),
                    MercCampCaptures = table.Column<int>(type: "int(11)", nullable: false),
                    WatchTowerCaptures = table.Column<int>(type: "int(11)", nullable: false),
                    MetaExperience = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.ReplayID, x.PlayerID });
                    table.ForeignKey(
                        name: "FK_ReplayCharacterScoreResult_ReplayCharacter",
                        columns: x => new { x.ReplayID, x.PlayerID },
                        principalTable: "replaycharacter",
                        principalColumns: new[] { "ReplayID", "PlayerID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "replaycharactersilenced",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false),
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.ReplayID, x.PlayerID });
                    table.ForeignKey(
                        name: "FK_ReplayCharacterSilenced_ReplayCharacter",
                        columns: x => new { x.ReplayID, x.PlayerID },
                        principalTable: "replaycharacter",
                        principalColumns: new[] { "ReplayID", "PlayerID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "replaycharactertalent",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false),
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false),
                    TalentID = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.ReplayID, x.PlayerID, x.TalentID });
                    table.ForeignKey(
                        name: "FK_ReplayCharacterTalent_Player",
                        column: x => x.PlayerID,
                        principalTable: "player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReplayCharacterTalent_Replay",
                        column: x => x.ReplayID,
                        principalTable: "replay",
                        principalColumn: "ReplayID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReplayCharacterTalent_ReplayCharacter",
                        columns: x => new { x.ReplayID, x.PlayerID },
                        principalTable: "replaycharacter",
                        principalColumns: new[] { "ReplayID", "PlayerID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "replaycharacterupgradeeventreplaylengthpercent",
                columns: table => new
                {
                    ReplayID = table.Column<int>(type: "int(11)", nullable: false),
                    PlayerID = table.Column<int>(type: "int(11)", nullable: false),
                    UpgradeEventType = table.Column<int>(type: "int(11)", nullable: false),
                    UpgradeEventValue = table.Column<int>(type: "int(11)", nullable: false),
                    ReplayLengthPercent = table.Column<decimal>(type: "decimal(15,13)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.ReplayID, x.PlayerID, x.UpgradeEventType, x.UpgradeEventValue });
                    table.ForeignKey(
                        name: "FK_this_ReplayCharacter",
                        columns: x => new { x.ReplayID, x.PlayerID },
                        principalTable: "replaycharacter",
                        principalColumns: new[] { "ReplayID", "PlayerID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "FK_EventIDParent_EventID_idx",
                table: "event",
                column: "EventIDParent");

            migrationBuilder.CreateIndex(
                name: "EventName_UNIQUE",
                table: "event",
                column: "EventName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupFinderListingTypeID",
                table: "groupfinderlisting",
                column: "GroupFinderListingTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_TimestampExpiration",
                table: "groupfinderlisting",
                column: "TimestampExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_Character_ReplayBuildFirst",
                table: "herotalentinformation",
                columns: new[] { "Character", "ReplayBuildFirst" });

            migrationBuilder.CreateIndex(
                name: "IX_Character_TalentID",
                table: "herotalentinformation",
                columns: new[] { "Character", "TalentID" });

            migrationBuilder.CreateIndex(
                name: "IX_ReplayBuildFirst_ReplayBuildLast",
                table: "herotalentinformation",
                columns: new[] { "ReplayBuildFirst", "ReplayBuildLast" });

            migrationBuilder.CreateIndex(
                name: "IX_Character_ReplayBuildFirst_ReplayBuildLast",
                table: "herotalentinformation",
                columns: new[] { "Character", "ReplayBuildFirst", "ReplayBuildLast" });

            migrationBuilder.CreateIndex(
                name: "replays_created_at_index",
                table: "hotsapireplays",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "replays_filename_unique",
                table: "hotsapireplays",
                column: "filename",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "replays_fingerprint_v3_index",
                table: "hotsapireplays",
                column: "fingerprint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "replays_game_date_index",
                table: "hotsapireplays",
                column: "game_date");

            migrationBuilder.CreateIndex(
                name: "replays_game_type_index",
                table: "hotsapireplays",
                column: "game_type");

            migrationBuilder.CreateIndex(
                name: "replays_parsed_id_uindex",
                table: "hotsapireplays",
                column: "parsed_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "replays_processed_deleted_index",
                table: "hotsapireplays",
                columns: new[] { "processed", "deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_IsEligibleForLeaderboard",
                table: "leaderboardranking",
                column: "IsEligibleForLeaderboard");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueID",
                table: "leaderboardranking",
                column: "LeagueID");

            migrationBuilder.CreateIndex(
                name: "IX_GameMode_CurrentMMR",
                table: "leaderboardranking",
                columns: new[] { "GameMode", "CurrentMMR" });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueID_LeagueRank",
                table: "leaderboardranking",
                columns: new[] { "LeagueID", "LeagueRank" });

            migrationBuilder.CreateIndex(
                name: "IX_GameMode_LeagueID_LeagueRank",
                table: "leaderboardranking",
                columns: new[] { "GameMode", "LeagueID", "LeagueRank" });

            migrationBuilder.CreateIndex(
                name: "AttributeName_UNIQUE",
                table: "localizationalias",
                column: "AttributeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Group",
                table: "localizationalias",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_Type",
                table: "localizationalias",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_DateTimeErrorOccurred",
                table: "logerror",
                column: "DateTimeErrorOccurred");

            migrationBuilder.CreateIndex(
                name: "IX_UserHostAddress",
                table: "logerror",
                column: "UserHostAddress");

            migrationBuilder.CreateIndex(
                name: "FK_LogError_my_aspnet_users_idx",
                table: "logerror",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "Name_UNIQUE",
                table: "mountinformation",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "email_UNIQUE",
                table: "net48_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "FK_player_idx",
                table: "net48_users",
                column: "playerID");

            migrationBuilder.CreateIndex(
                name: "username_UNIQUE",
                table: "net48_users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BattleNetId",
                table: "player",
                column: "BattleNetId");

            migrationBuilder.CreateIndex(
                name: "IX_Name",
                table: "player",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BattleNetRegionId_BattleNetSubId",
                table: "player",
                columns: new[] { "BattleNetRegionId", "BattleNetSubId" });

            migrationBuilder.CreateIndex(
                name: "IX_BattleNetRegionId_PlayerID",
                table: "player",
                columns: new[] { "BattleNetRegionId", "PlayerID" });

            migrationBuilder.CreateIndex(
                name: "Unique_BattleNet",
                table: "player",
                columns: new[] { "BattleNetRegionId", "BattleNetSubId", "BattleNetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "FK_PlayerAggregate_LocalizationAlias_idx",
                table: "playeraggregate",
                column: "FavoriteCharacter");

            migrationBuilder.CreateIndex(
                name: "IX_TimestampLastUpdated",
                table: "playeraggregate",
                column: "TimestampLastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_GameMode_TimestampLastUpdated",
                table: "playeraggregate",
                columns: new[] { "GameMode", "TimestampLastUpdated" });

            migrationBuilder.CreateIndex(
                name: "FK_PlayerIDAlt_idx",
                table: "playeralt",
                column: "PlayerIDAlt");

            migrationBuilder.CreateIndex(
                name: "FK_PlayerIDMain",
                table: "playeralt",
                column: "PlayerIDMain");

            migrationBuilder.CreateIndex(
                name: "IX_MilestoneDate",
                table: "playermmrmilestonev3",
                column: "MilestoneDate");

            migrationBuilder.CreateIndex(
                name: "IX_MMRRating",
                table: "playermmrmilestonev3",
                column: "MMRRating");

            migrationBuilder.CreateIndex(
                name: "IX_GameMode_MilestoneDate",
                table: "playermmrmilestonev3",
                columns: new[] { "GameMode", "MilestoneDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerID_MilestoneDate",
                table: "playermmrmilestonev3",
                columns: new[] { "PlayerID", "MilestoneDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Email",
                table: "premiumpayment",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_GameMode",
                table: "replay",
                column: "GameMode");

            migrationBuilder.CreateIndex(
                name: "IX_MapID",
                table: "replay",
                column: "MapID");

            migrationBuilder.CreateIndex(
                name: "ReplayHash_UNIQUE",
                table: "replay",
                column: "ReplayHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimestampCreated",
                table: "replay",
                column: "TimestampCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TimestampReplay",
                table: "replay",
                column: "TimestampReplay");

            migrationBuilder.CreateIndex(
                name: "IX_GameMode_TimestampReplay",
                table: "replay",
                columns: new[] { "GameMode", "TimestampReplay" });

            migrationBuilder.CreateIndex(
                name: "IX_ReplayBuild_TimestampReplay",
                table: "replay",
                columns: new[] { "ReplayBuild", "TimestampReplay" });

            migrationBuilder.CreateIndex(
                name: "IX_GameMode_TimestampReplay_ReplayID",
                table: "replay",
                columns: new[] { "GameMode", "TimestampReplay", "ReplayID" });

            migrationBuilder.CreateIndex(
                name: "IX_GameMode",
                table: "replay_dups",
                column: "GameMode");

            migrationBuilder.CreateIndex(
                name: "IX_MapID",
                table: "replay_dups",
                column: "MapID");

            migrationBuilder.CreateIndex(
                name: "ReplayHash_UNIQUE",
                table: "replay_dups",
                column: "ReplayHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimestampCreated",
                table: "replay_dups",
                column: "TimestampCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TimestampReplay",
                table: "replay_dups",
                column: "TimestampReplay");

            migrationBuilder.CreateIndex(
                name: "IX_GameMode_TimestampReplay",
                table: "replay_dups",
                columns: new[] { "GameMode", "TimestampReplay" });

            migrationBuilder.CreateIndex(
                name: "IX_ReplayBuild_TimestampReplay",
                table: "replay_dups",
                columns: new[] { "ReplayBuild", "TimestampReplay" });

            migrationBuilder.CreateIndex(
                name: "IX_GameMode_TimestampReplay_ReplayID",
                table: "replay_dups",
                columns: new[] { "GameMode", "TimestampReplay", "ReplayID" });

            migrationBuilder.CreateIndex(
                name: "FK_ReplayCharacter_LocalizationAlias_idx",
                table: "replaycharacter",
                column: "CharacterID");

            migrationBuilder.CreateIndex(
                name: "IX_Character_IsWinner",
                table: "replaycharacter",
                column: "IsWinner");

            migrationBuilder.CreateIndex(
                name: "IX_MMRBefore",
                table: "replaycharacter",
                column: "MMRBefore");

            migrationBuilder.CreateIndex(
                name: "FK_ReplayCharacter_Player_idx",
                table: "replaycharacter",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "FK_ReplayCharacter_Replay_idx",
                table: "replaycharacter",
                column: "ReplayID");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterID_CharacterLevel",
                table: "replaycharacter",
                columns: new[] { "CharacterID", "CharacterLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterID_IsWinner",
                table: "replaycharacter",
                columns: new[] { "CharacterID", "IsWinner" });

            migrationBuilder.CreateIndex(
                name: "IX_ReplayID_CharacterID",
                table: "replaycharacter",
                columns: new[] { "ReplayID", "CharacterID" });

            migrationBuilder.CreateIndex(
                name: "FK_ReplayCharacter_Player_idx",
                table: "replaycharacterdraftorder",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "FK_ReplayCharacter_Replay_idx",
                table: "replaycharacterdraftorder",
                column: "ReplayID");

            migrationBuilder.CreateIndex(
                name: "IX_MatchAwardType",
                table: "replaycharactermatchaward",
                column: "MatchAwardType");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerID",
                table: "replaycharactermatchaward",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayID",
                table: "replaycharactermatchaward",
                column: "ReplayID");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerID_MatchAwardType",
                table: "replaycharactermatchaward",
                columns: new[] { "PlayerID", "MatchAwardType" });

            migrationBuilder.CreateIndex(
                name: "IX_ReplayID_PlayerID",
                table: "replaycharactermatchaward",
                columns: new[] { "ReplayID", "PlayerID" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerID",
                table: "replaycharactertalent",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayID",
                table: "replaycharactertalent",
                column: "ReplayID");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayID_PlayerID",
                table: "replaycharactertalent",
                columns: new[] { "ReplayID", "PlayerID" });

            migrationBuilder.CreateIndex(
                name: "IX_UpgradeEventType",
                table: "replaycharacterupgradeeventreplaylengthpercent",
                column: "UpgradeEventType");

            migrationBuilder.CreateIndex(
                name: "IX_UpgradeEventType_UpgradeEventValue",
                table: "replaycharacterupgradeeventreplaylengthpercent",
                columns: new[] { "UpgradeEventType", "UpgradeEventValue" });

            migrationBuilder.CreateIndex(
                name: "IX_ReplayID",
                table: "replayperiodicxpbreakdown",
                column: "ReplayID");

            migrationBuilder.CreateIndex(
                name: "IX_IsWinner_GameTimeMinute",
                table: "replayperiodicxpbreakdown",
                columns: new[] { "IsWinner", "GameTimeMinute" });

            migrationBuilder.CreateIndex(
                name: "FK_ReplayShare_Player_idx",
                table: "replayshare",
                column: "PlayerIDSharedBy");

            migrationBuilder.CreateIndex(
                name: "FK_ReplayShare_Replay_idx",
                table: "replayshare",
                column: "ReplayID");

            migrationBuilder.CreateIndex(
                name: "IX_Title",
                table: "replayshare",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_UpvoteScore",
                table: "replayshare",
                column: "UpvoteScore");

            migrationBuilder.CreateIndex(
                name: "IX_UpvoteScore_Title",
                table: "replayshare",
                columns: new[] { "UpvoteScore", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterID",
                table: "replayteamheroban",
                column: "CharacterID");

            migrationBuilder.CreateIndex(
                name: "IX_IsWinner",
                table: "replayteamheroban",
                column: "IsWinner");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayID_IsWinner",
                table: "replayteamheroban",
                columns: new[] { "ReplayID", "IsWinner" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerID",
                table: "replayteamobjective",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_TeamObjectiveType",
                table: "replayteamobjective",
                column: "TeamObjectiveType");

            migrationBuilder.CreateIndex(
                name: "idx_talentimagemapping_TalentName_HeroName",
                table: "talentimagemapping",
                columns: new[] { "TalentName", "HeroName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "email_UNIQUE",
                table: "zamuser",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "amazonreplacementbucket");

            migrationBuilder.DropTable(
                name: "blogposts");

            migrationBuilder.DropTable(
                name: "buildnumbers");

            migrationBuilder.DropTable(
                name: "dataupdate");

            migrationBuilder.DropTable(
                name: "event");

            migrationBuilder.DropTable(
                name: "fingerprint_date");

            migrationBuilder.DropTable(
                name: "groupfinderlisting");

            migrationBuilder.DropTable(
                name: "heroiconinformation");

            migrationBuilder.DropTable(
                name: "herotalentinformation");

            migrationBuilder.DropTable(
                name: "hotsapireplays");

            migrationBuilder.DropTable(
                name: "hotsapitalents");

            migrationBuilder.DropTable(
                name: "leaderboardoptout");

            migrationBuilder.DropTable(
                name: "leaderboardranking");

            migrationBuilder.DropTable(
                name: "logerror");

            migrationBuilder.DropTable(
                name: "missingtalents");

            migrationBuilder.DropTable(
                name: "mmrrecalc");

            migrationBuilder.DropTable(
                name: "mountinformation");

            migrationBuilder.DropTable(
                name: "playeraggregate");

            migrationBuilder.DropTable(
                name: "playeralt");

            migrationBuilder.DropTable(
                name: "playerbanned");

            migrationBuilder.DropTable(
                name: "playerbannedleaderboard");

            migrationBuilder.DropTable(
                name: "playerdisablenamechange");

            migrationBuilder.DropTable(
                name: "playermmrmilestonev3");

            migrationBuilder.DropTable(
                name: "playermmrreset");

            migrationBuilder.DropTable(
                name: "premiumpayment");

            migrationBuilder.DropTable(
                name: "replay_dups");

            migrationBuilder.DropTable(
                name: "replay_dups2");

            migrationBuilder.DropTable(
                name: "replay_mirror");

            migrationBuilder.DropTable(
                name: "replay_notalents");

            migrationBuilder.DropTable(
                name: "replay_playertalentbuilds");

            migrationBuilder.DropTable(
                name: "replaycharacterdraftorder");

            migrationBuilder.DropTable(
                name: "replaycharactermatchaward");

            migrationBuilder.DropTable(
                name: "replaycharacterscoreresult");

            migrationBuilder.DropTable(
                name: "replaycharactersilenced");

            migrationBuilder.DropTable(
                name: "replaycharactertalent");

            migrationBuilder.DropTable(
                name: "replaycharacterupgradeeventreplaylengthpercent");

            migrationBuilder.DropTable(
                name: "replayperiodicxpbreakdown");

            migrationBuilder.DropTable(
                name: "replayshare");

            migrationBuilder.DropTable(
                name: "replayteamheroban");

            migrationBuilder.DropTable(
                name: "replayteamobjective");

            migrationBuilder.DropTable(
                name: "reputation");

            migrationBuilder.DropTable(
                name: "talentimagemapping");

            migrationBuilder.DropTable(
                name: "unknowndata");

            migrationBuilder.DropTable(
                name: "votes");

            migrationBuilder.DropTable(
                name: "zamuser");

            migrationBuilder.DropTable(
                name: "league");

            migrationBuilder.DropTable(
                name: "net48_users");

            migrationBuilder.DropTable(
                name: "localizationalias");

            migrationBuilder.DropTable(
                name: "replaycharacter");

            migrationBuilder.DropTable(
                name: "player");

            migrationBuilder.DropTable(
                name: "replay");
        }
    }
}
