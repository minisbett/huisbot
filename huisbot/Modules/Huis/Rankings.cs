using Discord.Interactions;
using huisbot.Enums;
using huisbot.Models.Huis;
using huisbot.Modules.Autocompletes;
using huisbot.Services;
using static System.Formats.Asn1.AsnWriter;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the rankings group & player and score subcommand, displaying the global leaderboard in a rework.
/// </summary>
[Group("rankings", "Commands for the global player/score rankings of a rework.")]
public class RankingsCommandModule : HuisModuleBase
{
  public RankingsCommandModule(HuisApiService huis) : base(huis) { }

  [SlashCommand("player", "Displays the global player leaderboard of the specified rework.")]
  public async Task HandlePlayerAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("sort", "The sorting and order for the players. Defaults to sort by New PP Descending.")]
    [Autocomplete(typeof(PlayerSortAutocompleteHandler))] string sortId = "new_pp_incl_bonus_desc",
    [Summary("onlyUpToDate", "Bool whether outdated players/uncalculated will be included. Defaults to true.")] bool onlyUpToDate = true,
    [Summary("hideUnranked", "Bool whether inactive players should be excluded. Defaults to true.")] bool hideUnranked = true,
    [Summary("page", "The page of the players. 1 page displays 10 players.")] [MinValue(1)] int page = 1)
  {
    await DeferAsync();

    // Get the sorting options.
    HuisPlayerSort? sort = await GetPlayerSortAsync(sortId);
    if (sort is null)
      return;

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // Get the player rankings.
    HuisPlayer[]? players = await GetPlayerRankingsAsync(rework.Id, sort, onlyUpToDate, hideUnranked);
    if (players is null)
      return;

    // Return the embed to the user.
    await FollowupAsync(embed: Embeds.PlayerRankings(players, rework, page));
  }

  [SlashCommand("score", "Displays the global score leaderboard of the specified rework.")]
  public async Task HandleScoreAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("sort", "The sorting and order for the scores.  Defaults to sort by Local PP Descending.")]
    [Autocomplete(typeof(ScoreSortAutocompleteHandler))] string sortId = "local_pp_desc",
    [Summary("page", "The page of the scores. 1 page displays 10 scores.")] [MinValue(1)] int page = 1)
  {
    await DeferAsync();

    // Get the sorting options.
    HuisScoreSort? sort = await GetScoreSortAsync(sortId);
    if (sort is null)
      return;

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // Get the score rankings.
    HuisScore[]? scores = await GetScoreRankingsAsync(rework.Id, sort);
    if (scores is null)
      return;

    // Return the embed to the user.
    await FollowupAsync(embed: Embeds.ScoreRankings(scores, rework, page));
  }
}
