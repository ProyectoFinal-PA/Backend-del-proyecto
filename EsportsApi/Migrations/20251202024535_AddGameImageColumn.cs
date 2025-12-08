using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddGameImageColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameImageUrl",
                table: "Tournaments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameImageUrl",
                table: "Tournaments");
        }
    }
}
