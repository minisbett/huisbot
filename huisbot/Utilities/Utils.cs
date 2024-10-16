using Discord;
using Discord.Interactions;
using System.Text.RegularExpressions;

namespace huisbot.Utilities;

/// <summary>
/// Provides utility methods for any complex maths.
/// </summary>
internal static class Utils
{
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