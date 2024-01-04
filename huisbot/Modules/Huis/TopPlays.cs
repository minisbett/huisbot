﻿using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Utility;
using huisbot.Modules.Autocompletes;
using huisbot.Services;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the topplays command, displaying the top plays of a player in a rework.
/// </summary>
public class TopPlaysCommandModule : ModuleBase
{
  public TopPlaysCommandModule(HuisApiService huis, OsuApiService osu, PersistenceService persistence) : base(huis, osu, persistence) { }

  [SlashCommand("topplays", "Displays the top plays of you or the specified player in the specified rework.")]
  public async Task HandleScoreAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocomplete))] string reworkId,
    [Summary("player", "The osu! ID or name of the player. Optional, defaults to your linked osu! user.")] string? playerId = null,
    [Summary("page", "The page of the scores. 1 page displays 10 scores.")][MinValue(1)] int page = 1,
    [Summary("sort", "The sorting for the scores. Defaults to sort by Local PP.")]
    [Autocomplete(typeof(ProfileScoresSortAutocomplete))] string sortId = "local_pp_desc")
  {
    await DeferAsync();

    // Get the sorting option.
    Sort? sort = await GetSortAsync(sortId, Sort.ProfileScores);
    if (sort is null)
      return;

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // If no player identifier was specified, try to get one from a link.
    if (playerId is null)
    {
      // Get the link and check whether the request was successful.
      OsuDiscordLink? link = await GetOsuDiscordLinkAsync();
      if (link is null)
        return;

      // Set the player identifier to the linked osu! user ID. After that, a player will be retrieved from the osu! API.
      playerId = link.OsuId.ToString();
    }

    // Get the osu! user.
    OsuUser? user = await GetOsuUserAsync(playerId);
    if (user is null)
      return;

    // Get the player in the current rework.
    HuisPlayer? player = await GetHuisPlayerAsync(user.Id, rework, user.Name ?? "");
    if (player is null)
      return;

    // Get the top plays of the player.
    HuisScore[]? scores = await GetTopPlaysAsync(user, rework.Id);
    if (scores is null)
      return;

    // Apply the sorting to the scores, since this is done inside the browser on Huis and has no API parameter.
    Func<HuisScore, double> selector = sort.Code switch
    {
      "live_pp" => x => x.LivePP,
      "pp_diff" => x => x.LocalPP - x.LivePP,
      _ => x => x.LocalPP
    };
    HuisScore[] sortedScores = (sort.IsAscending ? scores.OrderBy(selector) : scores.OrderByDescending(selector)).ToArray();

    // Return the embed to the user.
    await FollowupAsync(embed: Embeds.TopPlays(user, scores, sortedScores, rework, sort, page));
  }
}
