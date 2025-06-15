using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LudzValorant.Migrations
{
    /// <inheritdoc />
    public partial class updatelevelrank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RankName",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RankName",
                table: "Accounts");
        }
    }
}
