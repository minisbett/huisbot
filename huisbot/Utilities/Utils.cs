using MathNet.Numerics;

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
  public static double? CalculateEstimatedUR(int count300, int count100, int count50, int misses, int circleCount, int sliderCount, double overallDifficulty, double clockRate)
  {
    // If there's no hits at all, the UR is infinity.
    if (count50 + count100 + count300 == 0)
      return double.PositiveInfinity;

    // Calculate the hit windows from the overall difficulty and clock rate.
    double hitWindow300 = 80 - 6 * overallDifficulty;
    double hitWindow100 = (140 - 8 * ((80 - hitWindow300 * clockRate) / 6)) / clockRate;
    double hitWindow50 = (200 - 10 * ((80 - hitWindow300 * clockRate) / 6)) / clockRate;

    int missCountCircles = Math.Min(misses, circleCount);
    int mehCountCircles = Math.Min(count50, circleCount - missCountCircles);
    int okCountCircles = Math.Min(count100, circleCount - missCountCircles - mehCountCircles);
    int greatCountCircles = Math.Max(0, circleCount - missCountCircles - mehCountCircles - okCountCircles);

    // Assume 100s, 50s, and misses happen on circles. If there are less non-300s on circles than 300s, compute the deviation on circles.
    if (greatCountCircles > 0)
    {
      // The probability that a player hits a circle is unknown, but we can estimate it to be the number of greats
      // on circles divided by the number of circles, and then add one to the number of circles as a bias correction.
      double greatProbabilityCircle = greatCountCircles / (circleCount - missCountCircles - mehCountCircles + 1.0);

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
    int missCountSliders = Math.Min(sliderCount, misses - missCountCircles);
    int greatCountSliders = sliderCount - missCountSliders;

    // We only get here if nothing was hit. In this case, there is no estimate for deviation.
    // Note that this is never negative, so checking if this is only equal to 0 makes sense.
    if (greatCountSliders == 0)
      return null;

    double greatProbabilitySlider = greatCountSliders / (sliderCount + 1.0);
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
}
