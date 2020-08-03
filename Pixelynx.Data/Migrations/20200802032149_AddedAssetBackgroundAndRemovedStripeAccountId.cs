using Microsoft.EntityFrameworkCore.Migrations;

namespace Pixelynx.Data.Migrations
{
    public partial class AddedAssetBackgroundAndRemovedStripeAccountId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeAccountId",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "Background",
                table: "Assets",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Background",
                table: "Assets");

            migrationBuilder.AddColumn<string>(
                name: "StripeAccountId",
                table: "Users",
                type: "text",
                nullable: true);
        }
    }
}
