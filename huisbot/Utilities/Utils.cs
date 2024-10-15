using Discord;
using Discord.Interactions;
using Discord.WebSocket;
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
  /// Tries to find osu! score information from common Discord bots in the last 100 messages of the channel of the interaction context.
  /// </summary>
  /// <param name="interaction">The interaction context of the command execution.</param>
  /// <returns>The embed score info.</returns>
  public static async Task<EmbedScoreInfo?> FindOsuBotScore(SocketInteractionContext interaction)
  {
    foreach (IMessage message in await interaction.Channel.GetMessagesAsync(100).FlattenAsync())
    {
      // If the message is from the bot itself, ignore it.
      if (message.Author.Id == interaction.Client.CurrentUser.Id)
        continue;

      // Go through all embeds in the message and check if the author URL or normal URL of any of them contain a beatmap URL.
      IEmbed? beatmapEmbed = null;
      int? beatmapId = null;
      foreach (IEmbed embed in message.Embeds)
        foreach (string str in new string[] { embed.Author?.Url ?? "", embed.Url ?? "" })
          if (str.StartsWith("https://osu.ppy.sh/b/") && int.TryParse(str.Split('/').Last(), out int id))
            (beatmapId, beatmapEmbed) = (id, embed);

      // If no beatmap URL was found, continue with the next message.
      if (beatmapId is null)
        continue;

      // Try to find further information in the embed by generating a big score info string with the author, description and fields.
      string? scoreInfo = beatmapEmbed!.Author + "\n" + beatmapEmbed.Description + "\n"
                        + string.Join("\n", beatmapEmbed.Fields.Select(x => $"{x.Name}\n{x.Value}"));
      scoreInfo = scoreInfo.Replace("**", ""); // We ignore any bold text

      // Try to find hits in the format of " [300/100/50/miss]" (owo) or " {300/100/50/miss}" (bathbot).
      Match match = Regex.Match(scoreInfo, "[\\[{]\\s*\\d+\\/(\\d+)\\/(\\d+)\\/(\\d+)\\s*[\\]}]");
      int? count100 = match.Success ? int.Parse(match.Groups[1].Value) : null;
      int? count50 = match.Success ? int.Parse(match.Groups[2].Value) : null;
      int? misses = match.Success ? int.Parse(match.Groups[3].Value) : null;

      // Try to find a combo in the format of " x<number>/<number> " (owo) or " <number>x/<number>x " (bathbot).
      match = Regex.Match(scoreInfo, "x(\\d+)\\/\\d+|(\\d+)x\\/\\d+x");
      int? combo = match.Success ? int.Parse(match.Groups[2].Value != "" ? match.Groups[2].Value : match.Groups[1].Value) : null;

      // Try to find the mods in the format of " +<mod1><mod2...> ".
      match = Regex.Match(scoreInfo, "\\+\\s*([A-Z]+)");
      string? mods = match.Success ? match.Groups[1].Value : null;

      // If not all information was found, only return the beatmap id.
      if (combo is null || count100 is null || count50 is null || misses is null || mods is null)
        return new EmbedScoreInfo(beatmapId.Value);

      // Return the embed score info with all found values.
      return new EmbedScoreInfo(beatmapId.Value, count100.Value, count50.Value, misses.Value, combo.Value, mods);
    }

    return null;
  }
}

/// <summary>
/// Represents a score parsed from a Discord embed from another osu!-related Discord bot.
/// If any of <see cref="Count100"/>, <see cref="Count50"/>, <see cref="Misses"/>, <see cref="Combo"/> or <see cref="Mods"/>
/// is null, all of them will be null and only the beatmap id is provided, since a complete score could not be parsed.
/// </summary>
/// <param name="BeatmapId">The ID of the beatmap.</param>
/// <param name="Count100">The amount of 100s in the score.</param>
/// <param name="Count50">The amount of 50s in the score.</param>
/// <param name="Misses">The amount of misses in the score.</param>
/// <param name="Combo">The amount of combo in the score.</param>
/// <param name="Mods">The mods applied to the score.</param>
public record EmbedScoreInfo(int BeatmapId, int? Count100 = null, int? Count50 = null, int? Misses = null,
                             int? Combo = null, string? Mods = null);