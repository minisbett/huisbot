using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Modules.Autocompletes;
using huisbot.Services;
using huisbot.Utils;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the simulate command, calculating the score of a player in a rework.
/// </summary>
public class SimulateCommandModule : ModuleBase
{
  private readonly HuisApiService _huis;

  public SimulateCommandModule(HuisApiService huis, OsuApiService osu, PersistenceService persistence) : base(huis, osu, persistence)
  {
    _huis = huis;
  }

  [SlashCommand("simulate", "Simulates a score in the specified rework with the specified parameters.")]
  public async Task HandleAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocomplete))] string reworkId,
    [Summary("score", "The ID or alias of a score to base the score attributes off. Can be overriden by other parameters.")] string? scoreId = null,
    [Summary("beatmap", "The ID or alias of the beatmap.")] string? beatmapId = null,
    [Summary("combo", "The maximum combo in the score.")] int? combo = null,
    [Summary("100s", "The amount of 100s/oks in the score.")] int? count100 = null,
    [Summary("50s", "The amount of 50s/mehs in the score.")] int? count50 = null,
    [Summary("misses", "The amount of misses in the score.")] int? misses = null,
    [Summary("mods", "The mods used in the score.")] string? mods = null)
  {
    await DeferAsync();
    mods = mods?.ToUpper();

    // Check if either a beatmap ID or a score ID was specified.
    if (beatmapId is null && scoreId is null)
    {
      await FollowupAsync(embed: Embeds.Error("Either a beatmap ID or a score ID must be specified."));
      return;
    }

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // Disallow non-Onion users to access Onion-level reworks.
    if (rework.IsOnionLevel && !await IsOnionAsync())
    {
      await FollowupAsync(embed: Embeds.NotOnion);
      return;
    }

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
      mods ??= string.Join("", score.Mods);
    }

    // Default mods to "" if they haven't been initialized by score parsing before.
    mods ??= "";

    // Get the beatmap from the identifier.
    OsuBeatmap? beatmap = await GetBeatmapAsync(beatmapId!);
    if (beatmap is null)
      return;

    // Get the difficulty rating of the beatmap.
    double? difficultyRating = await GetDifficultyRatingAsync(rework.RulesetId, beatmap.Id, mods);
    if (difficultyRating is null)
      return;

    // Construct the HuisCalculationRequest.
    HuisCalculationRequest request = new HuisCalculationRequest(beatmap.Id, rework)
    {
      Combo = combo,
      Count100 = count100,
      Count50 = count50,
      Misses = misses,
      Mods = ModUtils.Split(mods)
    };

    // Display the calculation progress in an embed to the user.
    IUserMessage msg = await FollowupAsync(embed: Embeds.Calculating(false, rework.IsLive));

    // Get the local result from the Huis API and check whether it was successful.
    HuisCalculatedScore? localScore = await _huis.CalculateAsync(request);
    if (localScore is null)
    {
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError("Failed to calculate the local score with the Huis API."));
      return;
    }

    // If the requested rework is the live rework, the calculation is done here, therefore set the live score to the local one.
    HuisCalculatedScore? liveScore = localScore;
    if (!rework.IsLive)
    {
      // Switch the branch of the request to the live "rework" and update the calculation progress embed.
      request.Rework = live;
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.Calculating(true, false));

      // Get the live result from the Huis API and check whether it was successful.
      liveScore = await _huis.CalculateAsync(request);
      if (liveScore is null)
      {
        await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError("Failed to calculate the live score with the Huis API."));
        return;
      }
    }

    // Send the result in an embed to the user.
    await ModifyOriginalResponseAsync(x => x.Embed = Embeds.CalculatedScore(localScore, liveScore, rework, beatmap, difficultyRating.Value));
  }
}
