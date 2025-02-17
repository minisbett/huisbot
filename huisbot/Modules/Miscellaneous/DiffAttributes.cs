using Discord;
using Discord.Interactions;
using huisbot.Helpers;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Modules.Huis;
using huisbot.Utilities;

namespace huisbot.Modules.Miscellaneous;

public partial class MiscellaneousCommandModule
{
  /// <summary>
  /// The partial interaction module for the effmisscount command.
  /// </summary>
  [IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
  [CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
  [SlashCommand("diffattributes", "Calculates the effective misscount based off the comboes, slider count, 100s & 50s and misses.")]
  public async Task HandleDiffAttributesAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId = "master",
    [Summary("beatmap", "The ID, URL or alias of the beatmap.")] string? beatmapId = null,
    [Summary("mods", "The mods applied to the beatmap.")] string? modsStr = null,
    [Summary("clockRate", "The clock rate of the score. Automatically adds DT/HT.")][MinValue(0.01)][MaxValue(2)] double clockRate = 1)
  {
    await DeferAsync();

    // Check if either a beatmap ID was specified, or if a recent bot message with a beatmap URL can be found.
    if (beatmapId is null)
    {
      // Look for a message with a score in the channel.
      if (await Utils.FindOsuBotScore(Context) is EmbedScoreInfo score)
      {
        beatmapId = score.BeatmapId.ToString();
        modsStr ??= score.Mods;
      }

      // If there was no beatmap ID found, respond with an error.
      if (beatmapId is null)
      {
        await FollowupAsync(embed: Embeds.Error("Please specify a beatmap."));
        return;
      }
    }

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // Parse the mod-related parameters.
    OsuMods mods = OsuMods.FromString(modsStr ?? "");
    mods.SetClockRate(clockRate);

    // Get the beatmap from the identifier.
    OsuBeatmap? beatmap = await GetBeatmapAsync(beatmapId!);
    if (beatmap is null)
      return;

    // Construct the calculation request.
    HuisCalculationRequest request = new(beatmap, rework, mods);

    // Display the calculation progress in an embed to the user.
    IUserMessage msg = await FollowupAsync(embed: Embeds.Calculating(rework, null, false));

    // Get the result from the Huis API and check whether it was successful.
    HuisCalculationResponse? localScore = await CalculateScoreAsync(request);
    if (localScore is null)
      return;

    // Send the result in an embed to the user.
    await ModifyOriginalResponseAsync(x => x.Embed = Embeds.DifficultyAttributes(localScore, rework, beatmap));
  }
}
