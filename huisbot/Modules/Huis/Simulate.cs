using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Services;
using huisbot.Utilities;
using huisbot.Utilities.Discord;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the simulate command, calculating the score of a player in a rework.
/// </summary>
public class SimulateCommandModule : ModuleBase
{
  public SimulateCommandModule(HuisApiService huis, OsuApiService osu, PersistenceService persistence) : base(huis, osu, persistence) { }

  [SlashCommand("simulate", "Simulates a score in the specified rework with the specified parameters.")]
  public async Task HandleAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("score", "The ID or alias of a score to base the score attributes off. Can be overriden by other parameters.")] string? scoreId = null,
    [Summary("beatmap", "The ID or alias of the beatmap.")] string? beatmapId = null,
    [Summary("combo", "The maximum combo in the score.")] int? combo = null,
    [Summary("100s", "The amount of 100s/oks in the score.")] int? count100 = null,
    [Summary("50s", "The amount of 50s/mehs in the score.")] int? count50 = null,
    [Summary("misses", "The amount of misses in the score.")] int? misses = null,
    [Summary("mods", "The mods used in the score.")] string? modsStr = null)
  {
    await DeferAsync();

    // Check if either a beatmap ID or a score ID was specified, or if a recent bot message with a beatmap URL can be found.
    if (beatmapId is null && scoreId is null)
    {
      // Look for a message with a score in the last 100 messages.
      foreach (IMessage message in (await Context.Channel.GetMessagesAsync(100).FlattenAsync()))
        if (Utils.TryFindScore(message, out (int? beatmapId, int? score100, int? score50, int? scoreMiss, int? combo, string? mods) score))
        {
          beatmapId = score.beatmapId.ToString();
          combo ??= score.combo;
          count100 ??= score.score100;
          count50 ??= score.score50;
          misses ??= score.scoreMiss;
          modsStr ??= score.mods;
          break;
        }


      // If there was no beatmap ID found in the last 100 messages, respond with an error.
      if (beatmapId is null)
      {
        await FollowupAsync(embed: Embeds.Error("Either a beatmap ID or a score ID must be specified."));
        return;
      }
    }

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // Get the live rework, since the HuisRework object is required for score calculation.
    HuisRework? live = await GetReworkAsync(HuisRework.LiveId.ToString());
    if (live is null)
      return;

    // If a score was specified, get the score and fill the unset parameters with it's attributes.
    if (scoreId is not null)
    {
      OsuScore? score = await GetScoreAsync(rework.RulesetId, scoreId);
      if (score is null)
        return;

      // Replace all unset parameters with the attributes of the score.
      beatmapId = score.Beatmap.Id.ToString();
      combo ??= score.MaxCombo;
      count100 ??= score.Statistics.Count100;
      count50 ??= score.Statistics.Count50;
      misses ??= score.Statistics.Misses;
      modsStr ??= string.Join("", score.Mods);
    }

    // Parse the mods into a mods object.
    Mods mods = Mods.Parse(modsStr ?? "");

    // Get the beatmap from the identifier.
    OsuBeatmap? beatmap = await GetBeatmapAsync(beatmapId!);
    if (beatmap is null)
      return;

    // Get the difficulty rating of the beatmap.
    double? difficultyRating = await GetDifficultyRatingAsync(rework.RulesetId, beatmap.Id, mods);
    if (difficultyRating is null)
      return;

    // Construct the HuisCalculationRequest.
    HuisSimulationRequest request = new HuisSimulationRequest(beatmap.Id, rework)
    {
      Combo = combo,
      Count100 = count100,
      Count50 = count50,
      Misses = misses,
      Mods = mods.Array
    };

    // Display the calculation progress in an embed to the user.
    IUserMessage msg = await FollowupAsync(embed: Embeds.Calculating(false, rework.IsLive));

    // Get the local result from the Huis API and check whether it was successful.
    HuisSimulatedScore? localScore = await SimulateScoreAsync(request);
    if (localScore is null)
      return;

    // If the requested rework is the live rework, the calculation is done here, therefore set the live score to the local one.
    HuisSimulatedScore? liveScore = localScore;
    if (!rework.IsLive)
    {
      // Switch the branch of the request to the live "rework" and update the calculation progress embed.
      request.Rework = live;
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.Calculating(true, false));

      // Get the live result from the Huis API and check whether it was successful.
      liveScore = await SimulateScoreAsync(request);
      if (liveScore is null)
        return;
    }

    // Send the result in an embed to the user.
    await ModifyOriginalResponseAsync(x => x.Embed = Embeds.CalculatedScore(localScore, liveScore, rework, beatmap, difficultyRating.Value));
  }
}
