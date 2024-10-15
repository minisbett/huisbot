﻿using Discord;
using Discord.Interactions;
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
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("score", "The ID or alias of a score to get beatmap and mods from. Mods are overriden by the mods parameter.")] string? scoreId = null,
    [Summary("beatmap", "The ID or alias of the beatmap.")] string? beatmapId = null,
    [Summary("mods", "The mods applied to the beatmap.")] string? modsStr = null)
  {
    await DeferAsync();

    // Check if either a beatmap ID or a score ID was specified, or if a recent bot message with a beatmap URL can be found.
    if (beatmapId is null && scoreId is null)
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
        await FollowupAsync(embed: Embeds.Error("Either a beatmap ID or a score ID must be specified."));
        return;
      }
    }

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // If a score was specified, get the score and fill the unset parameters with it's attributes.
    if (scoreId is not null)
    {
      OsuScore? score = await GetScoreAsync(scoreId);
      if (score is null)
        return;

      // Replace all unset parameters with the attributes of the score.
      beatmapId = score.Beatmap.Id.ToString();
      modsStr ??= score.Mods.ToString();
    }

    // Parse the mods into a mods object.
    Mods mods = Mods.Parse(modsStr ?? "");

    // Get the beatmap from the identifier.
    OsuBeatmap? beatmap = await GetBeatmapAsync(beatmapId!);
    if (beatmap is null)
      return;

    // Construct the simulation request.
    HuisSimulationRequest request = new(beatmap.Id, rework, mods.Array);

    // Display the simulation progress in an embed to the user.
    IUserMessage msg = await FollowupAsync(embed: Embeds.Simulating(rework, null, false));

    // Get the result from the Huis API and check whether it was successful.
    HuisSimulationResponse? localScore = await SimulateScoreAsync(request);
    if (localScore is null)
      return;

    // Send the result in an embed to the user.
    await ModifyOriginalResponseAsync(x => x.Embed = Embeds.DifficultyAttributes(localScore, rework, beatmap));
  }
}
