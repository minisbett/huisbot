using Discord;
using Discord.Interactions;
using huisbot.Models.Osu;
using huisbot.Utilities;

namespace huisbot.Modules.Miscellaneous;

/// <summary>
/// The partial interaction module for the effmisscount command.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public partial class MiscellaneousCommandModule : ModuleBase
{
  [SlashCommand("effmisscount", "Calculates the effective misscount based off the comboes, slider count, 100s & 50s and misses.")]
  public async Task HandleEffMissCountAsync(
    [Summary("combo", "The combo of the theoretical score.")][MinValue(0)] int combo,
    [Summary("maxCombo", "The maximum combo of the beatmap.")][MinValue(1)] int? maxCombo = null,
    [Summary("sliderCount", "The slider count of the beatmap.")][MinValue(0)] int? sliderCount = null,
    [Summary("100s50s", "The amount of 100s + 50s.")][MinValue(0)] int hits = 0,
    [Summary("misses", "The amount of misses.")][MinValue(0)] int misses = 0,
    [Summary("beatmap", "The ID or alias of the beatmap to get the maximum combo and slider count from.")] string? beatmapId = null)
  {
    await DeferAsync();

    // Check if either a beatmap ID or a maximum combo and slider count were specified.
    if (beatmapId is null && (maxCombo is null || sliderCount is null))
    {
      await FollowupAsync(embed: Embeds.Error("Either a beatmap or a maximum combo and slider count must be specified."));
      return;
    }

    // If a beatmap was specified, get the beatmap and set the maximum combo and slider count.
    if (beatmapId is not null)
    {
      OsuBeatmap? beatmap = await GetBeatmapAsync(beatmapId);
      if (beatmap is null)
        return;

      maxCombo = beatmap.MaxCombo;
      sliderCount = beatmap.SliderCount;
    }

    // Calculate the combo based misscount and full combo threshold.
    double comboBasedMissCount = 0;
    double fullComboThreshold = 0;
    if (sliderCount > 0)
    {
      fullComboThreshold = maxCombo!.Value - 0.1 * sliderCount.Value;
      if (combo < fullComboThreshold)
        comboBasedMissCount = fullComboThreshold / Math.Max(1.0, combo);
    }

    // Calculate the effective misscount.
    double effMissCount = Math.Max(misses, Math.Min(comboBasedMissCount, hits));

    // Return the effective miss count in an embed.
    await FollowupAsync(embed: Embeds.EffMissCount(combo, maxCombo!.Value, sliderCount!.Value, hits, misses, comboBasedMissCount, fullComboThreshold, effMissCount));
  }
}
