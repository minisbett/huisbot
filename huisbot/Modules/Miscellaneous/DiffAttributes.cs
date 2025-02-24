using Discord;
using Discord.Interactions;
using huisbot.Helpers;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Modules.Huis;

namespace huisbot.Modules.Miscellaneous;

public partial class MiscellaneousCommandModule
{
  /// <summary>
  /// The partial interaction module for the diffattributes command.
  /// </summary>
  [IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
  [CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
  [SlashCommand("diffattributes", "Calculates the effective misscount based off the comboes, slider count, 100s & 50s and misses.")]
  public async Task HandleDiffAttributesAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId = "master",
    [Summary("beatmap", "The ID, URL or alias of the beatmap.")] string? beatmapId = null,
    [Summary("mods", "The mods applied to the beatmap.")] string? modsStr = null,
    [Summary("clockRate", "The clock rate of the score. Automatically adds DT/HT.")][MinValue(0.5)][MaxValue(2)] double clockRate = 1)
  {
    await DeferAsync();

    // If no beatmap ID was specified, find a message by an osu! bot.
    if (beatmapId is null)
    {
      if (await Utils.FindOsuBotScore(Context) is EmbedScoreInfo score)
      {
        beatmapId = score.BeatmapId.ToString();
        modsStr ??= score.Mods;
      }

      if (beatmapId is null)
      {
        await FollowupAsync(embed: Embeds.Error("Please specify a beatmap."));
        return;
      }
    }

    if (await GetReworkAsync(reworkId) is not HuisRework rework) return;
    if (await GetBeatmapAsync(beatmapId) is not OsuBeatmap beatmap) return;

    // Build an OsuMods object based on the specified parameters.
    OsuMods mods = OsuMods.FromString(modsStr ?? "");
    mods.SetClockRate(clockRate);

    IUserMessage msg = await FollowupAsync(embed: Embeds.Calculating(rework, null, false));

    if (await CalculateScoreAsync(new(beatmap, rework, mods)) is not HuisCalculationResponse localScore) return;

    await ModifyOriginalResponseAsync(x => x.Embed = Embeds.DifficultyAttributes(localScore, rework, beatmap));
  }
}
