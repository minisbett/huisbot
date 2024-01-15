using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace huisbot.Migrations
{
    /// <inheritdoc />
    public partial class AddCachedScoreSimulations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cached_score_simulations",
                columns: table => new
                {
                    request_identifier = table.Column<string>(type: "TEXT", nullable: false),
                    score_json = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cached_score_simulations", x => x.request_identifier);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cached_score_simulations");
        }
    }
}
