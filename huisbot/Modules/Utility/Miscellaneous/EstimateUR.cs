using Discord.Interactions;
using huisbot.Models.Osu;
using huisbot.Services;
using MathNet.Numerics;
using System.Diagnostics.Metrics;

namespace huisbot.Modules.Utility.Miscellaneous;

/// <summary>
/// The partial interaction module for the estimateur command.
/// </summary>
public partial class MiscellaneousCommandModule : ModuleBase
{
  [SlashCommand("estimateur", "Calculates the effective misscount based off the comboes, slider count, 100s & 50s and misses.")]
  public async Task HandleEstimateURAsync(
    [Summary("beatmap", "The ID or alias of the beatmap to get the beatmap information. Is overidden by the score parameter.")] string? beatmapId = null,
    [Summary("score", "The ID or alias of the score to get the beatmap & stats from. Can be overriden by other parameters.")] string? scoreId = null,
    [Summary("300s", "The 300s of the simulated score.")] int? count300 = null,
    [Summary("100s", "The 100s of the simulated score.")] int? count100 = null,
    [Summary("50s", "The 50s of the simulated score.")] int? count50 = null,
    [Summary("misses", "The misses of the simulated score.")] int? misses = null,
    [Summary("clockRate", "The clock rate of the simulated score.")][Choice("No Mod (x1)", 1)][Choice("Double Time (x1.5)", 1)][Choice("Half Time (x0.75)", 1)]
    double? clockRate = null)
  {
    await DeferAsync();

    // Check if either a beatmap or a score was specified.
    if (beatmapId is null && scoreId is null)
    {
      await FollowupAsync(embed: Embeds.Error("Either a score or a beatmap must be specified."));
      return;
    }

    // If a score was specified, get the score and fill the unset parameters with it's beatmap & statistics.
    if (scoreId is not null)
    {
      OsuScore? score = await GetScoreAsync(0, scoreId);
      if (score is null)
        return;

      beatmapId = score.Beatmap.Id.ToString();
      count300 ??= score.Statistics.Count300;
      count100 ??= score.Statistics.Count100;
      count50 ??= score.Statistics.Count50;
      misses ??= score.Statistics.Misses;
      clockRate ??= !score.Mods.Contains("HT") ? score.Mods.Contains("DT") || score.Mods.Contains("NC") ? 1.5 : 1 : 0.75;
    }

    // Get the beatmap from the identifier.
    OsuBeatmap? beatmap = await GetBeatmapAsync(beatmapId!);
    if (beatmap is null)
      return;

    // Default the parameters to a NM SS score if they haven't been set via the score parameter or the command parameters.
    count300 ??= beatmap.CircleCount + beatmap.SliderCount + beatmap.SpinnerCount;
    count100 ??= 0;
    count50 ??= 0;
    misses ??= 0;
    clockRate ??= 1;

    // Calculate the estimated UR.
    #region UR Estimation
    double? ur = null;

    // Calculate the hit windows.
    double hitWindow300 = 80 - 6 * beatmap.OverallDifficulty;
    double hitWindow100 = (140 - 8 * ((80 - hitWindow300 * clockRate.Value) / 6)) / clockRate.Value;
    double hitWindow50 = (200 - 10 * ((80 - hitWindow300 * clockRate.Value) / 6)) / clockRate.Value;

    // Calculate the maximum amount of circles that might have been hit with the given judgement.
    int missCountCircles = Math.Min(misses.Value, beatmap.CircleCount);
    int mehCountCircles = Math.Min(count50.Value, beatmap.CircleCount - missCountCircles);
    int okCountCircles = Math.Min(count100.Value, beatmap.CircleCount - missCountCircles - mehCountCircles);
    int greatCountCircles = Math.Max(0, beatmap.CircleCount - missCountCircles - mehCountCircles - okCountCircles);

    // Only proceed if anything has been hit.
    if (count50 + count100 + count300 > 0)
    {

      // Assume 100s, 50s, and misses happen on circles. If there are less non-300s on circles than 300s, compute the deviation on circles.
      if (greatCountCircles > 0)
      {
        // The probability that a player hits a circle is unknown, but we can estimate it to be the number of greats on
        // circles divided by the number of circles, and then add one to the number of circles as a bias correction.
        double greatProbabilityCircle = greatCountCircles / (beatmap.CircleCount - missCountCircles - mehCountCircles + 1.0);

        // Compute the deviation assuming 300s and 100s are normally distributed, and 50s are uniformly distributed.
        // Begin with the normal distribution first.
        double deviationOnCircles = hitWindow300 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(greatProbabilityCircle));
        deviationOnCircles *= Math.Sqrt(1 - Math.Sqrt(2 / Math.PI) * hitWindow100 * Math.Exp(-0.5 * Math.Pow(hitWindow100 / deviationOnCircles, 2))
            / (deviationOnCircles * SpecialFunctions.Erf(hitWindow100 / (Math.Sqrt(2) * deviationOnCircles))));

        // Then compute the variance for 50s.
        double mehVariance = (hitWindow50 * hitWindow50 + hitWindow100 * hitWindow50 + hitWindow100 * hitWindow100) / 3;

        // Find the total deviation.
        deviationOnCircles = Math.Sqrt(((greatCountCircles + okCountCircles) * Math.Pow(deviationOnCircles, 2) + mehCountCircles * mehVariance) / (greatCountCircles + okCountCircles + mehCountCircles));

        ur = deviationOnCircles * 10;
      }
      else
      {
        // If there are more non-300s than there are circles, compute the deviation on sliders instead.
        // Here, all that matters is whether or not the slider was missed, since it is impossible
        // to get a 100 or 50 on a slider by mis-tapping it.
        int missCountSliders = Math.Min(beatmap.CircleCount, misses.Value - missCountCircles);
        int greatCountSliders = beatmap.SliderCount - missCountSliders;

        // We only get here if nothing was hit. In this case, there is no estimate for deviation.
        // Note that this is never negative, so checking if this is only equal to 0 makes sense.
        if (greatCountSliders > 0)
        {

          double greatProbabilitySlider = greatCountSliders / (beatmap.CircleCount + 1.0);
          double deviationOnSliders = hitWindow50 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(greatProbabilitySlider));
          ur = deviationOnSliders * 10;
        }
      }
    }
    #endregion

    // Return the estimated UR in an embed.
    await FollowupAsync(embed: Embeds.EstimateUR(hitWindow300, hitWindow100, hitWindow50, ur));
  }
}
