using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualHair.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixUserHairstyleUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserHairstyles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HairstyleId = table.Column<int>(type: "int", nullable: false),
                    FacialHairId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHairstyles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHairstyles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserHairstyles_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserHairstyles_FacialHairs_FacialHairId",
                        column: x => x.FacialHairId,
                        principalTable: "FacialHairs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserHairstyles_Hairstyles_HairstyleId",
                        column: x => x.HairstyleId,
                        principalTable: "Hairstyles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserHairstyles_FacialHairId",
                table: "UserHairstyles",
                column: "FacialHairId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHairstyles_HairstyleId",
                table: "UserHairstyles",
                column: "HairstyleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHairstyles_UserId",
                table: "UserHairstyles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHairstyles_UserId1",
                table: "UserHairstyles",
                column: "UserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserHairstyles");
        }
    }
}
