using Discord;
using Discord.Interactions;
using huisbot.Helpers;
using huisbot.Models.Huis;
using huisbot.Models.Osu;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the simulate command, calculating the score of a user in a rework.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public class SimulateCommandModule(IServiceProvider services) : ModuleBase(services)
{
  [SlashCommand("simulate", "Simulates a score in the specified rework with the specified parameters.")]
  public async Task HandleAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId = "master",
    [Summary("referenceRework", "The reference rework to compare the score to. Defaults to the live PP system.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string? refReworkId = null,
    [Summary("beatmap", "The ID, URL or alias of the beatmap.")] string? beatmapId = null,
    [Summary("combo", "The maximum combo in the score.")] int? combo = null,
    [Summary("100s", "The amount of 100s/oks in the score.")] int? count100 = null,
    [Summary("50s", "The amount of 50s/mehs in the score.")] int? count50 = null,
    [Summary("misses", "The amount of misses in the score.")] int? misses = null,
    [Summary("largeTickMisses", "(Lazer) The amount of large tick misses in the score.")] int? largeTickMisses = null,
    [Summary("sliderTailMisses", "(Lazer) The amount of misses in the score.")] int? sliderTailMisses = null,
    [Summary("mods", "The mods used in the score.")] string? modsStr = null,
    [Summary("clockRate", "The clock rate of the score. Automatically adds DT/HT.")][MinValue(0.5)][MaxValue(2)] double clockRate = 1,
    [Summary("cs", "The circle size (CS) of the score. Automatically adds DA.")][MinValue(0)][MaxValue(11)] double? circleSize = null,
    [Summary("ar", "The approach rate (AR) of the score. Automatically adds DA.")][MinValue(-10)][MaxValue(11)] double? approachRate = null,
    [Summary("od", "The overall difficulty (OD) of the score. Automatically adds DA.")][MinValue(0)][MaxValue(11)] double? overallDifficulty = null)
  {
    await DeferAsync();

    // If no beatmap ID was specified, find a message by an osu! bot.
    if (beatmapId is null)
    {
      if (await Utils.FindOsuBotScore(Context) is EmbedScoreInfo score)
      {
        beatmapId = score.BeatmapId.ToString();
        combo ??= score.Combo;
        count100 ??= score.Statistics?.Count100;
        count50 ??= score.Statistics?.Count50;
        misses ??= score.Statistics?.Misses;
        modsStr ??= score.Mods;
      }

      if (beatmapId is null)
      {
        await FollowupAsync(embed: Embeds.Error("Please specify a beatmap."));
        return;
      }
    }

    if (await GetReworkAsync(reworkId) is not HuisRework rework) return;
    if (await GetReworkAsync(refReworkId ?? HuisRework.LiveId.ToString()) is not HuisRework refRework) return;
    if (await GetBeatmapAsync(beatmapId) is not OsuBeatmap beatmap) return;

    // Build an OsuMods object based on the specified parameters.
    OsuMods mods = OsuMods.FromString(modsStr ?? "");
    mods.SetClockRate(clockRate);
    if(circleSize is not null) mods.SetCS(circleSize.Value);
    if(approachRate is not null) mods.SetAR(approachRate.Value);
    if(overallDifficulty is not null) mods.SetOD(overallDifficulty.Value);

    IUserMessage msg = await FollowupAsync(embed: Embeds.Calculating(rework, rework == refRework ? null : refRework, false));

    int? sliderTailHits = sliderTailMisses is null ? null : beatmap.SliderCount - sliderTailMisses.Value;
    OsuScoreStatistics statistics = new(count100, count50, misses, largeTickMisses, sliderTailHits);
    HuisCalculationResponse? localScore = await CalculateScoreAsync(new(beatmap, rework, mods, combo, statistics));
    if (localScore is null)
      return;

    // If the requested rework is the same as the reference, set the scores equal and don't perform another calculation.
    HuisCalculationResponse? refScore = localScore;
    if (rework != refRework)
    {
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.Calculating(rework, refRework, true));

      if ((refScore = await CalculateScoreAsync(new(beatmap, refRework, mods, combo, statistics))) is null) return;
    }

    await ModifyOriginalResponseAsync(x => x.Embed = Embeds.CalculatedScore(localScore, refScore, rework, refRework, beatmap));
  }
}
