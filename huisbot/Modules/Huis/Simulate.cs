using Discord;
using Discord.Interactions;
using huisbot.Enums;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Modules.Autocompletes;
using huisbot.Services;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the simulate command, calculating the score of a player in a rework.
/// </summary>
public class SimulateCommandModule : HuisModuleBase
{
  private readonly HuisApiService _huis;

  public SimulateCommandModule(HuisApiService huis, OsuApiService osu, PersistenceService persistence) : base(huis, osu, persistence)
  {
    _huis = huis;
  }

  [SlashCommand("simulate", "Simulates a score in the specified rework with the specified parameters.")]
  public async Task HandleAsync(
    [Summary("beatmap", "The ID or alias of the beatmap.")] string beatmapId,
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("combo", "The maximum combo in the score.")] int? combo = null,
    [Summary("100s", "The amount of 100s/oks in the score.")] int? count100 = null,
    [Summary("50s", "The amount of 50s/mehs in the score.")] int? count50 = null,
    [Summary("misses", "The amount of misses in the score.")] int? misses = null,
    [Summary("mods", "The mods used in the score.")] string mods = "")
  {
    await DeferAsync();

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // Get the beatmap from the identifier.
    OsuBeatmap? beatmap = await GetBeatmapAsync(beatmapId);
    if (beatmap is null)
      return;

    // Get the difficulty rating of the beatmap.
    double? difficultyRating = await GetDifficultyRatingAsync(rework.RulesetId, beatmap.Id, mods);
    if (difficultyRating is null)
      return;

    // Construct the HuisCalculationRequest.
    HuisCalculationRequest request = new HuisCalculationRequest(beatmap.Id, rework.Code!)
    {
      Combo = combo,
      Count100 = count100,
      Count50 = count50,
      Misses = misses,
      Mods = OsuMod.Parse(mods).Select(x => x.Acronym).ToArray() // Parse them to OsuMods to filter out invalid mods.
    };

    // Display the calculation progress in an embed to the user.
    IUserMessage msg = await FollowupAsync(embed: Embeds.Calculating(false, rework.IsLive));

    // Get the local result from the Huis API and check whether it was successful.
    HuisCalculatedScore? local = await _huis.CalculateAsync(request);
    if (local is null)
    {
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError("Failed to calculate the local score with the Huis API."));
      return;
    }

    // If the requested rework is the live rework, the calculation is done here, therefore set the live score to the local one.
    HuisCalculatedScore? live = local;
    if (!rework.IsLive)
    {
      // Switch the branch of the request to the live "rework" and update the calculation progress embed.
      request.ReworkCode = "live";
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.Calculating(true, false));

      // Get the live result from the Huis API and check whether it was successful.
      live = await _huis.CalculateAsync(request);
      if (live is null)
      {
        await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError("Failed to calculate the live score with the Huis API."));
        return;
      }
    }

    // Send the result in an embed to the user.
    await ModifyOriginalResponseAsync(x => x.Embed = Embeds.CalculatedScore(local, live, rework, beatmap, difficultyRating.Value));
  }
}
