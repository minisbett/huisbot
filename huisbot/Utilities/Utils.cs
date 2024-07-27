using Discord;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using MathNet.Numerics;
using System.Text.RegularExpressions;

namespace huisbot.Utilities;

/// <summary>
/// Provides utility methods for any complex maths.
/// </summary>
internal static class Utils
{
  /// <summary>
  /// Estimates the player's tap deviation based on the OD, number of circles and sliders, and number of 300s, 100s, 50s, and misses,
  /// assuming the player's mean hit error is 0. The estimation is consistent in that two SS scores on the same map with the same settings
  /// will always return the same deviation. Sliders are treated as circles with a 50 hit window. Misses are ignored because they are usually due to misaiming.
  /// 300s and 100s are assumed to follow a normal distribution, whereas 50s are assumed to follow a uniform distribution.
  /// </summary>
  public static double? CalculateEstimatedUR(HuisSimulationResponse.HuisSimulationScoreStatistics statistics, OsuBeatmap beatmap, Mods mods)
  {
    // If there's no hits at all, the UR is infinity.
    if (statistics.Count50 + statistics.Count100 + statistics.Count300 == 0)
      return double.PositiveInfinity;

    // Calculate the hit windows from the overall difficulty and clock rate.
    double hitWindow300 = 80 - 6 * beatmap.GetAdjustedOD(mods);
    double hitWindow100 = (140 - 8 * ((80 - hitWindow300 * mods.ClockRate) / 6)) / mods.ClockRate;
    double hitWindow50 = (200 - 10 * ((80 - hitWindow300 * mods.ClockRate) / 6)) / mods.ClockRate;

    int missCountCircles = Math.Min(statistics.Misses, beatmap.CircleCount);
    int mehCountCircles = Math.Min(statistics.Count50, beatmap.CircleCount - missCountCircles);
    int okCountCircles = Math.Min(statistics.Count100, beatmap.CircleCount - missCountCircles - mehCountCircles);
    int greatCountCircles = Math.Max(0, beatmap.CircleCount - missCountCircles - mehCountCircles - okCountCircles);

    // Assume 100s, 50s, and misses happen on circles. If there are less non-300s on circles than 300s, compute the deviation on circles.
    if (greatCountCircles > 0)
    {
      // The probability that a player hits a circle is unknown, but we can estimate it to be the number of greats
      // on circles divided by the number of circles, and then add one to the number of circles as a bias correction.
      double greatProbabilityCircle = greatCountCircles / (beatmap.CircleCount - missCountCircles - mehCountCircles + 1.0);

      // Compute the deviation assuming 300s and 100s are normally distributed, and 50s are uniformly distributed. Begin with the normal distribution first.
      double deviationOnCircles = hitWindow300 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(greatProbabilityCircle));
      deviationOnCircles *= Math.Sqrt(1 - Math.Sqrt(2 / Math.PI) * hitWindow100 * Math.Exp(-0.5 * Math.Pow(hitWindow100 / deviationOnCircles, 2))
          / (deviationOnCircles * SpecialFunctions.Erf(hitWindow100 / (Math.Sqrt(2) * deviationOnCircles))));

      // Then compute the variance for 50s and find the total deviation.
      double mehVariance = (hitWindow50 * hitWindow50 + hitWindow100 * hitWindow50 + hitWindow100 * hitWindow100) / 3;
      deviationOnCircles = Math.Sqrt(((greatCountCircles + okCountCircles) * Math.Pow(deviationOnCircles, 2) + mehCountCircles * mehVariance) / (greatCountCircles + okCountCircles + mehCountCircles));

      // Multiply the deviation by 10 to get the UR.
      return deviationOnCircles * 10;
    }

    // If there are more non-300s than there are circles, compute the deviation on sliders instead. Here, all that matters is
    // whether or not the slider was missed, since it is impossible to get a 100 or 50 on a slider by mis-tapping it.
    int missCountSliders = Math.Min(beatmap.SliderCount, statistics.Misses - missCountCircles);
    int greatCountSliders = beatmap.SliderCount - missCountSliders;

    // We only get here if nothing was hit. In this case, there is no estimate for deviation.
    // Note that this is never negative, so checking if this is only equal to 0 makes sense.
    if (greatCountSliders == 0)
      return null;

    double greatProbabilitySlider = greatCountSliders / (beatmap.SliderCount + 1.0);
    double deviationOnSliders = hitWindow50 / (Math.Sqrt(2) * SpecialFunctions.ErfInv(greatProbabilitySlider));

    // Multiply the deviation by 10 to get the UR.
    return deviationOnSliders * 10;
  }

  /// <summary>
  /// Formats the specified alias to have a unified format, disregarding dashes, underscores, dots and spaces.
  /// </summary>
  /// <param name="alias">The alias.</param>
  /// <returns>The formatted alias.</returns>
  public static string GetFormattedAlias(string alias) => new string(alias.ToLower().Where(x => x is not ('-' or '_' or '.' or ' ')).ToArray());

  /// <summary>
  /// Tries to parse osu! score information from common Discord bots from the specified message.
  /// </summary>
  /// <param name="message">The discord message.</param>
  /// <param name="score">The parsed score.</param>
  /// <returns>Bool whether parsing was successful (a score was found).</returns>
  public static bool TryFindScore(IMessage message, out (int? beatmapId, int? count100, int? count50, int? countMiss, int? combo, string? mods) score)
  {
    score = (null, null, null, null, null, null);

    int? FindBeatmap(IEmbed embed) => new string[] { embed.Author?.Url ?? "", embed.Url ?? "" }
      .FirstOrDefault(x => x.StartsWith("https://osu.ppy.sh/b/"))?
      .Split('/').Last() is { } id ? int.Parse(id) : null;

    // Go through all embeds in the message and check if any of them contain a beatmap URL.
    IEmbed? beatmapEmbed = null;
    foreach (IEmbed embed in message.Embeds)
      (score.beatmapId, beatmapEmbed) = (FindBeatmap(embed), embed);

    // If no beatmap URL was found, return false.
    if (score.beatmapId is null)
      return false;

    // Try to find further information in the embed by generating a big score info string from the author, description and fields.
    string? scoreInfo = beatmapEmbed!.Author + "\n"
                      + beatmapEmbed.Description + "\n"
                      + string.Join("\n", beatmapEmbed.Fields.Select(x => $"{x.Name}\n{x.Value}"));
    scoreInfo = scoreInfo.Replace("**", ""); // bathbot puts combo in bold text

    // Try to find a combo in the format of " x<number>/<number> " (owo) or " <number>x/<number>x " (bathbot).
    Match match = Regex.Match(scoreInfo, "x(\\d+)\\/\\d+|(\\d+)x\\/\\d+x");
    string? combo = match.Groups.Count == 3 ? match.Groups[2].Value != "" ? match.Groups[2].Value : match.Groups[1].Value : null;
    
    // Try to find hits in the format of " [300/100/50/miss]" (owo) or " {300/100/50/miss}" (bathbot).
    match = Regex.Match(scoreInfo, "[\\[{]\\s*\\d+\\/(\\d+)\\/(\\d+)\\/(\\d+)\\s*[\\]}]");
    string? count100 = match.Groups.Count == 4 ? match.Groups[1].Value : null;
    string? count50 = match.Groups.Count == 4 ? match.Groups[2].Value : null;
    string? countMiss = match.Groups.Count == 4 ? match.Groups[3].Value : null;

    // Try to find the mods in the format of " +<mod1><mod2>... ".
    match = Regex.Match(scoreInfo, "\\+\\s*([A-Z]+)");
    string? mods = match.Groups.Count == 2 ? match.Groups[1].Value : null;

    // If not all information was found, return false.
    if (combo is null || count100 is null || count50 is null || countMiss is null || mods is null)
      return false;

    score = (score.beatmapId, int.Parse(count100), int.Parse(count50), int.Parse(countMiss), int.Parse(combo), mods);
    return true;
  }
}
