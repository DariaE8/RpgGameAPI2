using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    RequiredLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsSafeZone = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    AttackModifier = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HealthModifier = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Value = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enemies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Health = table.Column<int>(type: "integer", nullable: false, defaultValue: 50),
                    MaxHealth = table.Column<int>(type: "integer", nullable: false, defaultValue: 50),
                    Attack = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    ExperienceReward = table.Column<int>(type: "integer", nullable: false, defaultValue: 25),
                    GoldReward = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enemies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enemies_GameLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "GameLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Experience = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Health = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    MaxHealth = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    Attack = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    Gold = table.Column<int>(type: "integer", nullable: false, defaultValue: 50),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_GameLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "GameLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Objective = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TargetCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    CurrentCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ExperienceReward = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    GoldReward = table.Column<int>(type: "integer", nullable: false, defaultValue: 50),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Available"),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quests_GameLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "GameLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerEnemy",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnemyId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefeatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExperienceEarned = table.Column<int>(type: "integer", nullable: false),
                    GoldEarned = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerEnemy", x => new { x.PlayerId, x.EnemyId });
                    table.ForeignKey(
                        name: "FK_PlayerEnemy_Enemies_EnemyId",
                        column: x => x.EnemyId,
                        principalTable: "Enemies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerEnemy_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerInventory",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcquiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsEquipped = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerInventory", x => new { x.PlayerId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_PlayerInventory_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerInventory_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerQuest",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerQuest", x => new { x.PlayerId, x.QuestId });
                    table.ForeignKey(
                        name: "FK_PlayerQuest_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerQuest_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestEnemyRequirement",
                columns: table => new
                {
                    QuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnemyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequiredCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestEnemyRequirement", x => new { x.QuestId, x.EnemyId });
                    table.ForeignKey(
                        name: "FK_QuestEnemyRequirement_Enemies_EnemyId",
                        column: x => x.EnemyId,
                        principalTable: "Enemies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestEnemyRequirement_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestItemRequirement",
                columns: table => new
                {
                    QuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequiredCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestItemRequirement", x => new { x.QuestId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_QuestItemRequirement_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestItemRequirement_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enemies_Level",
                table: "Enemies",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Enemies_Level_Type",
                table: "Enemies",
                columns: new[] { "Level", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Enemies_LocationId",
                table: "Enemies",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Enemies_Name",
                table: "Enemies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Enemies_Type",
                table: "Enemies",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_GameLocations_IsSafeZone",
                table: "GameLocations",
                column: "IsSafeZone");

            migrationBuilder.CreateIndex(
                name: "IX_GameLocations_Name",
                table: "GameLocations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameLocations_RequiredLevel",
                table: "GameLocations",
                column: "RequiredLevel");

            migrationBuilder.CreateIndex(
                name: "IX_GameLocations_Type",
                table: "GameLocations",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Name",
                table: "Items",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Type",
                table: "Items",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Value",
                table: "Items",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEnemy_EnemyId",
                table: "PlayerEnemy",
                column: "EnemyId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInventory_ItemId",
                table: "PlayerInventory",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerQuest_QuestId",
                table: "PlayerQuest",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_CreatedAt",
                table: "Players",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Email",
                table: "Players",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_Level",
                table: "Players",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Players_LocationId",
                table: "Players",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Name",
                table: "Players",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_QuestEnemyRequirement_EnemyId",
                table: "QuestEnemyRequirement",
                column: "EnemyId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestItemRequirement_ItemId",
                table: "QuestItemRequirement",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_CreatedAt",
                table: "Quests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_ExperienceReward",
                table: "Quests",
                column: "ExperienceReward");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_LocationId",
                table: "Quests",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_Status",
                table: "Quests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_Title",
                table: "Quests",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerEnemy");

            migrationBuilder.DropTable(
                name: "PlayerInventory");

            migrationBuilder.DropTable(
                name: "PlayerQuest");

            migrationBuilder.DropTable(
                name: "QuestEnemyRequirement");

            migrationBuilder.DropTable(
                name: "QuestItemRequirement");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Enemies");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Quests");

            migrationBuilder.DropTable(
                name: "GameLocations");
        }
    }
}
