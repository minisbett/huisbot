using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Services;
using huisbot.Utilities;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the simulate command, calculating the score of a player in a rework.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public class SimulateCommandModule(HuisApiService huis, OsuApiService osu, PersistenceService persistence) : ModuleBase(huis, osu, persistence)
{
  [SlashCommand("simulate", "Simulates a score in the specified rework with the specified parameters.")]
  public async Task HandleAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("referenceRework", "The reference rework to compare the score to. Defaults to the live PP system.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string? reworkReferenceId = null,
    [Summary("score", "The ID or alias of a score to base the score attributes off. Can be overriden by other parameters.")] string? scoreId = null,
    [Summary("beatmap", "The ID or alias of the beatmap.")] string? beatmapId = null,
    [Summary("combo", "The maximum combo in the score.")] int? combo = null,
    [Summary("100s", "The amount of 100s/oks in the score.")] int? count100 = null,
    [Summary("50s", "The amount of 50s/mehs in the score.")] int? count50 = null,
    [Summary("misses", "The amount of misses in the score.")] int? misses = null,
    [Summary("mods", "The mods used in the score.")] string? modsStr = null)
  {
    await DeferAsync();

    // Default to the live PP system as the reference rework.
    reworkReferenceId ??= HuisRework.LiveId.ToString();

    // Check if either a beatmap ID or a score ID was specified, or if a recent bot message with a beatmap URL can be found.
    if (beatmapId is null && scoreId is null)
    {
      // Look for a message with a score in the channel.
      if (await Utils.FindOsuBotScore(Context) is EmbedScoreInfo score)
      {
        beatmapId = score.BeatmapId.ToString();
        combo ??= score.Combo;
        count100 ??= score.Count100;
        count50 ??= score.Count50;
        misses ??= score.Misses;
        modsStr ??= score.Mods;
      }

      // If there was no beatmap ID found, respond with an error.
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

    // Get the matching reference rework for the specified rework identifier.
    HuisRework? refRework = await GetReworkAsync(reworkReferenceId);
    if (refRework is null)
      return;

    // If a score was specified, get the score and fill the unset parameters with it's attributes.
    if (scoreId is not null)
    {
      OsuScore? score = await GetScoreAsync(scoreId);
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

    // Construct the simulation request.
    HuisSimulationRequest request = new(beatmap.Id, rework, mods.Array, combo, count100, count50, misses);

    // Display the simulation progress in an embed to the user.
    IUserMessage msg = await FollowupAsync(embed: Embeds.Simulating(rework, refRework, false));

    // Get the local result from the Huis API and check whether it was successful.
    HuisSimulationResponse? localScore = await SimulateScoreAsync(request);
    if (localScore is null)
      return;

    // If the requested rework is the same as the reference, simulation is done here.
    HuisSimulationResponse? refScore = localScore;
    if (rework != refRework)
    {
      // Update the simulation progress embed.
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.Simulating(rework, refRework, true));

      // Get the reference rework result from the Huis API and check whether it was successful.
      refScore = await SimulateScoreAsync(request.WithRework(refRework));
      if (refScore is null)
        return;
    }

    // Send the result in an embed to the user.
    await ModifyOriginalResponseAsync(x => x.Embed = Embeds.SimulatedScore(localScore, refScore, rework, refRework, beatmap));
  }
}
