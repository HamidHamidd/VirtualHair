using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualHair.Migrations
{
    /// <inheritdoc />
    public partial class MoveImagesToDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Hairstyles");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "FacialHairs");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "UserHairstyles",
                newName: "ContentType");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "UserHairstyles",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Hairstyles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Hairstyles",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "FacialHairs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "FacialHairs",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "UserHairstyles");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Hairstyles");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Hairstyles");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "FacialHairs");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "FacialHairs");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "UserHairstyles",
                newName: "ImagePath");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Hairstyles",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "FacialHairs",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);
        }
    }
}
