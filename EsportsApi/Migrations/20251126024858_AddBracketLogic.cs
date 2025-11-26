using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddBracketLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Partidas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NextMatchId",
                table: "Partidas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Round",
                table: "Partidas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "Partidas");

            migrationBuilder.DropColumn(
                name: "NextMatchId",
                table: "Partidas");

            migrationBuilder.DropColumn(
                name: "Round",
                table: "Partidas");
        }
    }
}
