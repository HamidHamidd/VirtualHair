using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualHair.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueTitleConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserHairstyles_UserId",
                table: "UserHairstyles");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserHairstyles",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "UserHairstyles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserHairstyles_UserId_Title",
                table: "UserHairstyles",
                columns: new[] { "UserId", "Title" },
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserHairstyles_UserId_Title",
                table: "UserHairstyles");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserHairstyles",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "UserHairstyles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_UserHairstyles_UserId",
                table: "UserHairstyles",
                column: "UserId");
        }
    }
}
