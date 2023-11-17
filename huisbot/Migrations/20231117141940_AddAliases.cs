using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace huisbot.Migrations
{
    /// <inheritdoc />
    public partial class AddAliases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "beatmap_aliases",
                columns: table => new
                {
                    alias = table.Column<string>(type: "TEXT", nullable: false),
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beatmap_aliases", x => x.alias);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "beatmap_aliases");
        }
    }
}
