using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTwitchChannelToPartida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TwitchChannelName",
                table: "Partidas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwitchChannelName",
                table: "Partidas");
        }
    }
}
