using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pixelynx.Data.Migrations
{
    public partial class AddedAssetUploader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UploaderId",
                table: "Assets",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Assets_UploaderId",
                table: "Assets",
                column: "UploaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Users_UploaderId",
                table: "Assets",
                column: "UploaderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Users_UploaderId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_UploaderId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "UploaderId",
                table: "Assets");
        }
    }
}
