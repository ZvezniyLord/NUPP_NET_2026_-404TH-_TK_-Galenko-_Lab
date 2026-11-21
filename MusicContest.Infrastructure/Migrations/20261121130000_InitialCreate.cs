using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MusicContest.Infrastructure.Data;

#nullable disable

namespace MusicContest.Infrastructure.Migrations;

// Початкова міграція EF Core для SQLite, написана у MigrationBuilder API.
[DbContext(typeof(MusicContestContext))]
[Migration("20261121130000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ContestParticipants",
            columns: table => new
            {
                DatabaseId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                StageName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Age = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ContestParticipants", x => x.DatabaseId));

        migrationBuilder.CreateTable(
            name: "Songs",
            columns: table => new
            {
                DatabaseId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                OriginalArtist = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                DurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                Key = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Songs", x => x.DatabaseId));

        migrationBuilder.CreateTable(
            name: "SoloSingers",
            columns: table => new
            {
                DatabaseId = table.Column<int>(type: "INTEGER", nullable: false),
                VoiceType = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                VocalRange = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                YearsOfExperience = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SoloSingers", x => x.DatabaseId);
                table.ForeignKey("FK_SoloSingers_ContestParticipants_DatabaseId",
                    x => x.DatabaseId, "ContestParticipants", "DatabaseId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "VocalGroups",
            columns: table => new
            {
                DatabaseId = table.Column<int>(type: "INTEGER", nullable: false),
                MemberCount = table.Column<int>(type: "INTEGER", nullable: false),
                Genre = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                HasBackingVocals = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VocalGroups", x => x.DatabaseId);
                table.ForeignKey("FK_VocalGroups_ContestParticipants_DatabaseId",
                    x => x.DatabaseId, "ContestParticipants", "DatabaseId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ParticipantProfiles",
            columns: table => new
            {
                DatabaseId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ParticipantDatabaseId = table.Column<int>(type: "INTEGER", nullable: false),
                Biography = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                ContactEmail = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ParticipantProfiles", x => x.DatabaseId);
                table.ForeignKey("FK_ParticipantProfiles_ContestParticipants_ParticipantDatabaseId",
                    x => x.ParticipantDatabaseId, "ContestParticipants", "DatabaseId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ParticipantSongs",
            columns: table => new
            {
                ParticipantDatabaseId = table.Column<int>(type: "INTEGER", nullable: false),
                SongDatabaseId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ParticipantSongs",
                    x => new { x.ParticipantDatabaseId, x.SongDatabaseId });
                table.ForeignKey("FK_ParticipantSongs_ContestParticipants_ParticipantDatabaseId",
                    x => x.ParticipantDatabaseId, "ContestParticipants", "DatabaseId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_ParticipantSongs_Songs_SongDatabaseId",
                    x => x.SongDatabaseId, "Songs", "DatabaseId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Performances",
            columns: table => new
            {
                DatabaseId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                ParticipantDatabaseId = table.Column<int>(type: "INTEGER", nullable: false),
                SongDatabaseId = table.Column<int>(type: "INTEGER", nullable: false),
                Score = table.Column<double>(type: "REAL", nullable: false),
                IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Performances", x => x.DatabaseId);
                table.ForeignKey("FK_Performances_ContestParticipants_ParticipantDatabaseId",
                    x => x.ParticipantDatabaseId, "ContestParticipants", "DatabaseId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_Performances_Songs_SongDatabaseId",
                    x => x.SongDatabaseId, "Songs", "DatabaseId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_ContestParticipants_Id", "ContestParticipants", "Id", unique: true);
        migrationBuilder.CreateIndex("IX_Songs_Id", "Songs", "Id", unique: true);
        migrationBuilder.CreateIndex("IX_ParticipantProfiles_Id", "ParticipantProfiles", "Id", unique: true);
        migrationBuilder.CreateIndex("IX_ParticipantProfiles_ParticipantDatabaseId", "ParticipantProfiles",
            "ParticipantDatabaseId", unique: true);
        migrationBuilder.CreateIndex("IX_ParticipantSongs_SongDatabaseId", "ParticipantSongs", "SongDatabaseId");
        migrationBuilder.CreateIndex("IX_Performances_Id", "Performances", "Id", unique: true);
        migrationBuilder.CreateIndex("IX_Performances_ParticipantDatabaseId", "Performances", "ParticipantDatabaseId");
        migrationBuilder.CreateIndex("IX_Performances_SongDatabaseId", "Performances", "SongDatabaseId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ParticipantProfiles");
        migrationBuilder.DropTable("ParticipantSongs");
        migrationBuilder.DropTable("Performances");
        migrationBuilder.DropTable("SoloSingers");
        migrationBuilder.DropTable("VocalGroups");
        migrationBuilder.DropTable("Songs");
        migrationBuilder.DropTable("ContestParticipants");
    }
}
