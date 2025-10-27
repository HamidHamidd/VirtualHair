using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualHair.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FacialHairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    DefaultColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacialHairs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hairstyles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DefaultFade = table.Column<int>(type: "int", nullable: false),
                    DefaultColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hairstyles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StoredPath = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPhotos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedLooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserPhotoId = table.Column<int>(type: "int", nullable: false),
                    HairstyleId = table.Column<int>(type: "int", nullable: true),
                    FacialHairId = table.Column<int>(type: "int", nullable: true),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    Fade = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    AdjustmentsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedLooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedLooks_FacialHairs_FacialHairId",
                        column: x => x.FacialHairId,
                        principalTable: "FacialHairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SavedLooks_Hairstyles_HairstyleId",
                        column: x => x.HairstyleId,
                        principalTable: "Hairstyles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SavedLooks_UserPhotos_UserPhotoId",
                        column: x => x.UserPhotoId,
                        principalTable: "UserPhotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedLooks_FacialHairId",
                table: "SavedLooks",
                column: "FacialHairId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedLooks_HairstyleId",
                table: "SavedLooks",
                column: "HairstyleId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedLooks_UserPhotoId",
                table: "SavedLooks",
                column: "UserPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPhotos_UserId_CreatedAt",
                table: "UserPhotos",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedLooks");

            migrationBuilder.DropTable(
                name: "FacialHairs");

            migrationBuilder.DropTable(
                name: "Hairstyles");

            migrationBuilder.DropTable(
                name: "UserPhotos");
        }
    }
}
