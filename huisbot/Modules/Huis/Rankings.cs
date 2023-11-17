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
public class RankingsCommandModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly HuisApiService _huis;

  public RankingsCommandModule(HuisApiService huis)
  {
    _huis = huis;
  }

  [SlashCommand("player", "Displays the global player leaderboard of the specified rework.")]
  public async Task HandlePlayerAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("sort", "The sorting and order for the players. Defaults to sort by New PP Descending.")]
    [Autocomplete(typeof(PlayerSortAutocompleteHandler))] string sortId = "new_pp_incl_bonus_desc",
    [Summary("onlyUpToDate", "Bool whether outdated players/uncalculated will be included. Defaults to false.")] bool onlyUpToDate = false,
    [Summary("hideUnranked", "Bool whether inactive players should be excluded. Defaults to false.")] bool hideUnranked = false,
    [Summary("page", "The page of the players. 1 page displays 10 players.")] [MinValue(1)] int page = 1)
  {
    await DeferAsync();

    // Try to find the specified sort by the specified identifier. If it doesn't exist, notify the user.
    HuisPlayerSort? sort = HuisPlayerSort.All.FirstOrDefault(x => x.Id == sortId || x.DisplayName == sortId);
    if (sort is null)
    {
      await FollowupAsync(embed: Embeds.Error($"Invalid sort type `{sortId}`."));
      return;
    }

    // Get all reworks, find the one with a matching identifier and check whether the process was successful. If not, notify the user.
    HuisRework[]? reworks = await _huis.GetReworksAsync();
    HuisRework? rework = reworks?.FirstOrDefault(x => x.Id.ToString() == reworkId || x.Code == reworkId || x.Name == reworkId);
    if (reworks is null)
    {
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the reworks from the Huis API."));
      return;
    }
    else if (rework is null)
    {
      await FollowupAsync(embed: Embeds.Error($"The rework `{reworkId}` could not be found."));
      return;
    }

    // Get the player rankings in the rework and check whether the request was successful. If not, notify the user.
    HuisPlayer[]? players = await _huis.GetPlayerRankingsAsync(rework.Id, sort, onlyUpToDate, hideUnranked);
    if (players is null)
    {
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the player rankings."));
      return;
    }

    // Return the embed to the user.
    int pageSize = 20;
    await FollowupAsync(embed: Embeds.PlayerRankings(players.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), rework, page));
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

    // Try to find the specified sort by the specified identifier. If it doesn't exist, notify the user.
    HuisScoreSort? sort = HuisScoreSort.All.FirstOrDefault(x => x.Id == sortId || x.DisplayName == sortId);
    if (sort is null)
    {
      await FollowupAsync(embed: Embeds.Error($"Invalid sort type `{sortId}`."));
      return;
    }

    // Get all reworks, find the one with a matching identifier and check whether the process was successful. If not, notify the user.
    HuisRework[]? reworks = await _huis.GetReworksAsync();
    HuisRework? rework = reworks?.FirstOrDefault(x => x.Id.ToString() == reworkId || x.Code == reworkId || x.Name == reworkId);
    if (reworks is null)
    {
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the reworks from the Huis API."));
      return;
    }
    else if (rework is null)
    {
      await FollowupAsync(embed: Embeds.Error($"The rework `{reworkId}` could not be found."));
      return;
    }

    // Get the score rankings in the rework and check whether the request was successful. If not, notify the user.
    HuisScore[]? scores = await _huis.GetScoreRankingsAsync(rework.Id, sort);
    if (scores is null)
    {
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the score rankings."));
      return;
    }

    // Return the embed to the user.
    await FollowupAsync(embed: Embeds.ScoreRankings(scores.Skip((page - 1) * 10).Take(10).ToArray(), rework, page));
  }
}
