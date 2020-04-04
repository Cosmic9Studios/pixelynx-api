using Microsoft.EntityFrameworkCore.Migrations;

namespace Pixelynx.Data.Migrations
{
    public partial class RemovedDefaultAssetPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Price",
                table: "Assets",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Price",
                table: "Assets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long));
        }
    }
}
