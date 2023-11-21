using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Modules.Utility;

/// <summary>
/// The interaction module for the misc group & various subcommands, providing miscellaneous utility commands.
/// </summary>
[Group("misc", "Miscellaneous utility commands.")]
public class MiscellaneousCommandModule : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand("effmisscount", "Calculates the effective misscount based off the comboes, slider count, 100s & 50s and misses.")]
  public async Task HandleEffectiveMissCountAsync(
    [Summary("combo", "The combo of the theoretical score.")][MinValue(0)] int combo,
    [Summary("maxCombo", "The combo of the beatmap.")][MinValue(1)] int maxCombo,
    [Summary("sliderCount", "The slider count of the beatmap.")][MinValue(0)] int sliderCount,
    [Summary("100s50s", "The amount of 100s + 50s.")][MinValue(0)] int hits = 0,
    [Summary("misses", "The amount of misses.")][MinValue(0)] int misses = 0)
  {
    await DeferAsync();

    // Calculate the combo based misscount and full combo threshold.
    double comboBasedMissCount = 0;
    double fullComboThreshold = 0;
    if (sliderCount > 0)
    {
      fullComboThreshold = maxCombo - 0.1 * sliderCount;
      if (combo < fullComboThreshold)
        comboBasedMissCount = fullComboThreshold / Math.Max(1.0, combo);
    }

    // Calculate the effective misscount.
    double effMissCount = Math.Max(misses, Math.Min(comboBasedMissCount, hits));

    // Return the effective miss count in an embed.
    await FollowupAsync(embed: Embeds.EffMissCount(combo, maxCombo, sliderCount, hits, misses, comboBasedMissCount, fullComboThreshold, effMissCount));
  }
}
