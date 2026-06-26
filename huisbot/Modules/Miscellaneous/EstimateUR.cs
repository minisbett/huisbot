using Discord.Interactions;
using huisbot.Models.Osu;
using huisbot.Services;
using MathNet.Numerics;

namespace huisbot.Modules.Miscellaneous;

/// <summary>
/// The partial interaction module for the estimateur command.
/// </summary>
public partial class MiscellaneousCommandModule : ModuleBase
{
  [SlashCommand("eur", "Calculates the effective misscount based off the comboes, slider count, 100s & 50s and misses.")]
  public async Task HandleEstimateURAsync(
    [Summary("beatmap", "The ID or alias of the beatmap to get the beatmap information. Is overidden by the score parameter.")] string? beatmapId = null,
    [Summary("score", "The ID or alias of the score to get the beatmap & stats from. Can be overriden by other parameters.")] string? scoreId = null,
    [Summary("300s", "The 300s in the score.")] int? count300 = null,
    [Summary("100s", "The 100s in the score.")] int? count100 = null,
    [Summary("50s", "The 50s in the score.")] int? count50 = null,
    [Summary("misses", "The misses in the score.")] int? misses = null,
    [Summary("clockRate", "The clock rate of the score.")] double? clockRate = null)
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
      if (await GetScoreAsync(scoreId) is not OsuScore score) return;

      beatmapId = score.Beatmap.Id.ToString();
      count300 ??= score.Statistics.Count300;
      count100 ??= score.Statistics.Count100;
      count50 ??= score.Statistics.Count50;
      misses ??= score.Statistics.Misses;
      clockRate ??= score.Mods.ClockRate;
    }
    
    // Get the beatmap from the identifier.
    if (await GetBeatmapAsync(beatmapId!) is not OsuBeatmap beatmap) return;

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
    double? greatProbabilityCircle = null;
    double? deviationOnCircles = null;
    double? mehVariance = null;
    double? greatProbabilitySlider = null;
    double? deviationOnSliders = null;
    
    // Only proceed if anything has been hit.
    if (count50 + count100 + count300 > 0)
    {

      // Assume 100s, 50s, and misses happen on circles. If there are less non-300s on circles than 300s, compute the deviation on circles.
      if (greatCountCircles > 0)
      {
        // The probability that a player hits a circle is unknown, but we can estimate it to be the number of greats on
        // circles divided by the number of circles, and then add one to the number of circles as a bias correction.
        greatProbabilityCircle = greatCountCircles / (beatmap.CircleCount - missCountCircles - mehCountCircles + 1.0);

        // Compute the deviation assuming 300s and 100s are normally distributed, and 50s are uniformly distributed.
        // Begin with the normal distribution first.
        deviationOnCircles = hitWindow300 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(greatProbabilityCircle.Value));
        deviationOnCircles = deviationOnCircles.Value * Math.Sqrt(1 - Math.Sqrt(2 / Math.PI) * hitWindow100 * Math.Exp(-0.5 * Math.Pow(hitWindow100 / deviationOnCircles.Value, 2))
            / (deviationOnCircles.Value * SpecialFunctions.Erf(hitWindow100 / (Math.Sqrt(2) * deviationOnCircles.Value))));

        // Then compute the variance for 50s.
        mehVariance = (hitWindow50 * hitWindow50 + hitWindow100 * hitWindow50 + hitWindow100 * hitWindow100) / 3;

        // Find the total deviation.
        deviationOnCircles = Math.Sqrt(((greatCountCircles + okCountCircles) * Math.Pow(deviationOnCircles.Value, 2) + mehCountCircles * mehVariance.Value) / (greatCountCircles + okCountCircles + mehCountCircles));

        ur = deviationOnCircles * 10;
      }
      else
      {
        // If there are more non-300s than there are circles, compute the deviation on sliders instead.
        // Here, all that matters is whether the slider was missed, since it is impossible
        // to get a 100 or 50 on a slider by mis-tapping it.
        int missCountSliders = Math.Min(beatmap.CircleCount, misses.Value - missCountCircles);
        int greatCountSliders = beatmap.SliderCount - missCountSliders;

        // We only get here if nothing was hit. In this case, there is no estimate for deviation.
        // Note that this is never negative, so checking if this is only equal to 0 makes sense.
        if (greatCountSliders > 0)
        {
          greatProbabilitySlider = greatCountSliders / (beatmap.CircleCount + 1.0);
          deviationOnSliders = hitWindow50 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(greatProbabilitySlider.Value));
          ur = deviationOnSliders * 10;
        }
      }
    }
    #endregion

    // Return the estimated UR in an embed.
    await FollowupAsync(embed: Embeds.EstimateUR(hitWindow300, hitWindow100, hitWindow50, mehVariance, missCountCircles, mehCountCircles, okCountCircles, 
      greatCountCircles, greatProbabilityCircle, greatProbabilitySlider, ur));
  }
}