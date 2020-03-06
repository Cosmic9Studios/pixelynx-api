using Microsoft.EntityFrameworkCore.Migrations;

namespace Pixelynx.Data.Migrations
{
    public partial class AddedAssetFileHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "Assets",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "Assets");
        }
    }
}
