using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pixelynx.Data.Migrations
{
    public partial class AddedAssetTypeAndParentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetType",
                table: "Assets",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Assets",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ParentId",
                table: "Assets",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Assets_ParentId",
                table: "Assets",
                column: "ParentId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Assets_ParentId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_ParentId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "AssetType",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Assets");
        }
    }
}
