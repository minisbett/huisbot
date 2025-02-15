using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;
using huisbot.Services;
using huisbot.Utilities;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the score command, calculating the existing score of a player.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
[Group("score", "Calculates a score in a rework based on the specified parameters.")]
public class ScoreCommandModule(HuisApiService huis, OsuApiService osu, PersistenceService persistence) : ModuleBase(huis, osu, persistence)
{
  private async Task HandleAsync(string reworkId, string? referenceReworkId, Task<OsuScore?> score, OsuUser? user = null)
  {
    // Default to the live PP system as the reference rework.
    referenceReworkId ??= HuisRework.LiveId.ToString();

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // Get the matching reference rework for the specified rework identifier.
    HuisRework? refRework = await GetReworkAsync(referenceReworkId);
    if (refRework is null)
      return;

    // Get the score from the specified task.
    await score;
    if (score.Result is null)
      return;

    // Get the user from the score. The user is only fetched from the API if not already provided when calling this method.
    user ??= await GetOsuUserAsync(score.Result.User.Id.ToString());
    if (user is null)
      return;

    // Get the beatmap from the identifier.
    OsuBeatmap? beatmap = await GetBeatmapAsync(score.Result.Beatmap.Id.ToString());
    if (beatmap is null)
      return;

    // Display the calculation progress in an embed to the user.
    IUserMessage msg = await FollowupAsync(embed: Embeds.Calculating(rework, rework == refRework ? null : refRework, false));

    // Get the local result from the Huis API and check whether it was successful.
    HuisCalculationResponse? localScore = await CalculateScoreAsync(new(beatmap, rework, score.Result.Mods, score.Result.MaxCombo, score.Result.Statistics));
    if (localScore is null)
      return;

    // If the requested rework is the same as the reference, calculation is done here.
    HuisCalculationResponse? refScore = localScore;
    if (rework != refRework)
    {
      // Update the calculation progress embed.
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.Calculating(rework, refRework, true));

      // Get the reference rework result from the Huis API and check whether it was successful.
      refScore = await CalculateScoreAsync(new(beatmap, refRework, score.Result.Mods, score.Result.MaxCombo, score.Result.Statistics));
      if (refScore is null)
        return;
    }

    // Send the result in an embed to the user.
    await ModifyOriginalResponseAsync(x => x.Embed = Embeds.CalculatedScore(localScore, refScore, rework, refRework, beatmap, score.Result, user));
  }

  [SlashCommand("id", "Calculates a score in a rework based on the specified ID.")]
  public async Task HandleIdAsync(
    [Summary("score", "The ID, URL or alias of the score.")] string scoreId,
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId = "master",
    [Summary("referenceRework", "The reference rework to compare the score to. Defaults to the live PP system.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string? referenceReworkId = null)
  {
    await DeferAsync();

    await HandleAsync(reworkId, referenceReworkId, GetScoreAsync(scoreId));
  }

  [SlashCommand("best", "Calculates the X-th best score of the specified user in a rework.")]
  public async Task HandleBestAsync(
    [Summary("player", "The osu! ID or name of the player. Optional, defaults to your linked osu! user.")] string? playerId = null,
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId = "master",
    [Summary("index", "The index of the score. Defaults to 1.")][MinValue(1)][MaxValue(100)] int index = 1,
    [Summary("referenceRework", "The reference rework to compare the score to. Defaults to the live PP system.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string? referenceReworkId = null)
  {
    await DeferAsync();

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

    // Get the osu! user by the identifier to get the user ID.
    OsuUser? user = await GetOsuUserAsync(playerId);
    if (user is null)
      return;

    await HandleAsync(reworkId, referenceReworkId, GetUserScoreAsync(user.Id, index, ScoreType.Best), user);
  }

  [SlashCommand("recent", "Calculates the X-th recent score of you or the specified user in a rework.")]
  public async Task HandleRecentAsync(
    [Summary("player", "The osu! ID or name of the player. Optional, defaults to your linked osu! user.")] string? playerId = null,
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId = "master",
    [Summary("index", "The index of the score. Defaults to 1.")][MinValue(1)][MaxValue(100)] int index = 1,
    [Summary("referenceRework", "The reference rework to compare the score to. Defaults to the live PP system.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string? referenceReworkId = null)
  {
    await DeferAsync();

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

    // Get the osu! user by the identifier to get the user ID.
    OsuUser? user = await GetOsuUserAsync(playerId);
    if (user is null)
      return;

    await HandleAsync(reworkId, referenceReworkId, GetUserScoreAsync(user.Id, index, ScoreType.Recent), user);
  }
}