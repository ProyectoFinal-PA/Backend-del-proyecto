using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsportsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizadorToTournament : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizadorId",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_OrganizadorId",
                table: "Tournaments",
                column: "OrganizadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_Users_OrganizadorId",
                table: "Tournaments",
                column: "OrganizadorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_Users_OrganizadorId",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_OrganizadorId",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "OrganizadorId",
                table: "Tournaments");
        }
    }
}
