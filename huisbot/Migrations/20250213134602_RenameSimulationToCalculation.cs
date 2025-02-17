using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace huisbot.Migrations
{
    /// <inheritdoc />
    public partial class RenameSimulationToCalculation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cached_score_simulations");

            migrationBuilder.CreateTable(
                name: "cached_score_calculations",
                columns: table => new
                {
                    request_identifier = table.Column<string>(type: "TEXT", nullable: false),
                    response_json = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cached_score_calculations", x => x.request_identifier);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cached_score_calculations");

            migrationBuilder.CreateTable(
                name: "cached_score_simulations",
                columns: table => new
                {
                    request_identifier = table.Column<string>(type: "TEXT", nullable: false),
                    response_json = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cached_score_simulations", x => x.request_identifier);
                });
        }
    }
}
