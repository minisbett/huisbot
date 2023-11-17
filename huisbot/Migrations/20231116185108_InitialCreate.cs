using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace huisbot.Migrations
{
  /// <inheritdoc />
  public partial class InitialCreate : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "osu_discord_links",
          columns: table => new
          {
            discord_id = table.Column<ulong>(type: "INTEGER", nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            osu_id = table.Column<int>(type: "INTEGER", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("pk_osu_discord_links", x => x.discord_id);
          });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "osu_discord_links");
    }
  }
}
