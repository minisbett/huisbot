using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Modules.Autocompletes;
using huisbot.Services;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the calculate command, calculating the score of a player in a rework.
/// </summary>
public class CalculateCommandModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly OsuApiService _osu;
  private readonly HuisApiService _huis;

  public CalculateCommandModule(OsuApiService osu, HuisApiService huis)
  {
    _osu = osu;
    _huis = huis;
  }

  [SlashCommand("calculate", "Calculates a score in the specified rework with the specified parameters.")]
  public async Task HandleAsync(
    [Summary("beatmap", "The ID of the beatmap.")] int beatmapId,
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("combo", "The maximum combo in the score.")] int? combo = null,
    [Summary("100s", "The amount of 100s/oks in the score.")] int? count100 = null,
    [Summary("50s", "The amount of 50s/mehs in the score.")] int? count50 = null,
    [Summary("misses", "The amount of misses in the score.")] int? misses = null,
    [Summary("mods", "The mods used in the score.")] string mods = "")
  {
    await DeferAsync();

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

    // Construct the HuisCalculationRequest.
    HuisCalculationRequest request = new HuisCalculationRequest(beatmapId, rework.Code!)
    {
      Combo = combo,
      Count100 = count100,
      Count50 = count50,
      Misses = misses,
      Mods = mods.Chunk(2).Select(x => new string(x)).ToArray()
    };

    // Display the calculation progress in an embed to the user.
    IUserMessage msg = await FollowupAsync(embed: Embeds.Calculating(false, false, rework.IsLive));

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
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.Calculating(true, false, false));

      // Get the live result from the Huis API and check whether it was successful.
      live = await _huis.CalculateAsync(request);
      if (live is null)
      {
        await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError("Failed to calculate the live score with the Huis API."));
        return;
      }
    }

    // Update the calculation progress embed again.
    await ModifyOriginalResponseAsync(x => x.Embed = Embeds.Calculating(true, true, rework.IsLive));

    // Get the beatmap from the osu! api and check whether it was successful.
    OsuBeatmap? beatmap = await _osu.GetBeatmapAsync(beatmapId);
    if (beatmap is null)
    {
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError("Failed to get the beatmap from the osu! API."));
      return;
    }

    // Send the result in an embed to the user.
    await ModifyOriginalResponseAsync(x => x.Embed = Embeds.CalculatedScore(local, live, rework, beatmap));
  }
}
