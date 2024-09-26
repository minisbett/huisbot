using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace huisbot.Migrations
{
  /// <inheritdoc />
  public partial class RenameScoreToResponse : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
          name: "score_json",
          table: "cached_score_simulations",
          newName: "response_json");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
          name: "response_json",
          table: "cached_score_simulations",
          newName: "score_json");
    }
  }
}
