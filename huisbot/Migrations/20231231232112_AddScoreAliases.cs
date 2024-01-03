using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace huisbot.Migrations
{
  /// <inheritdoc />
  public partial class AddScoreAliases : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
          name: "id",
          table: "beatmap_aliases",
          newName: "beatmap_id");

      migrationBuilder.CreateTable(
          name: "score_aliases",
          columns: table => new
          {
            alias = table.Column<string>(type: "TEXT", nullable: false),
            score_id = table.Column<long>(type: "INTEGER", nullable: false),
            display_name = table.Column<string>(type: "TEXT", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("pk_score_aliases", x => x.alias);
          });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "score_aliases");

      migrationBuilder.RenameColumn(
          name: "beatmap_id",
          table: "beatmap_aliases",
          newName: "id");
    }
  }
}
