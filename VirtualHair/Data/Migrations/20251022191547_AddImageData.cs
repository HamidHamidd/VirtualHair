using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualHair.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Hairstyle");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Fade");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Hairstyle",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Fade",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Hairstyle");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Fade");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Hairstyle",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Fade",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
