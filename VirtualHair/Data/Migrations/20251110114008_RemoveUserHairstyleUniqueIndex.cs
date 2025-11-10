using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualHair.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserHairstyleUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserHairstyles_UserId_Title",
                table: "UserHairstyles");

            migrationBuilder.CreateIndex(
                name: "IX_UserHairstyles_UserId",
                table: "UserHairstyles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserHairstyles_UserId",
                table: "UserHairstyles");

            migrationBuilder.CreateIndex(
                name: "IX_UserHairstyles_UserId_Title",
                table: "UserHairstyles",
                columns: new[] { "UserId", "Title" },
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }
    }
}
