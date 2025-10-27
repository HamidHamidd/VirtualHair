using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualHair.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathUploadSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Hairstyles");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "FacialHairs");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Hairstyles");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "FacialHairs");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Hairstyles",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "FacialHairs",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");
        }
    }
}
