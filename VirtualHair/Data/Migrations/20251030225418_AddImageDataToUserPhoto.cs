using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualHair.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageDataToUserPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoredPath",
                table: "UserPhotos");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "UserPhotos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "UserPhotos",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "UserPhotos",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "UserPhotoId",
                table: "UserHairstyles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserHairstyles_UserPhotoId",
                table: "UserHairstyles",
                column: "UserPhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserHairstyles_UserPhotos_UserPhotoId",
                table: "UserHairstyles",
                column: "UserPhotoId",
                principalTable: "UserPhotos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPhotos_AspNetUsers_UserId",
                table: "UserPhotos",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserHairstyles_UserPhotos_UserPhotoId",
                table: "UserHairstyles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPhotos_AspNetUsers_UserId",
                table: "UserPhotos");

            migrationBuilder.DropIndex(
                name: "IX_UserHairstyles_UserPhotoId",
                table: "UserHairstyles");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "UserPhotos");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "UserPhotos");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "UserPhotos");

            migrationBuilder.DropColumn(
                name: "UserPhotoId",
                table: "UserHairstyles");

            migrationBuilder.AddColumn<string>(
                name: "StoredPath",
                table: "UserPhotos",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");
        }
    }
}
