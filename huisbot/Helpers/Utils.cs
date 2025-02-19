using Discord;
using Discord.Interactions;
using huisbot.Models.Osu;
using System.Text.RegularExpressions;

namespace huisbot.Helpers;

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
  public static string GetFormattedAlias(string alias) => new(alias.ToLower().Where(x => x is not ('-' or '_' or '.' or ' ')).ToArray());

  /// <summary>
  /// Tries to find osu! score information from common Discord bots in the last 100 messages of the channel of the interaction context.
  /// </summary>
  /// <param name="interaction">The interaction context of the command execution.</param>
  /// <returns>The embed score info.</returns>
  public static async Task<EmbedScoreInfo?> FindOsuBotScore(SocketInteractionContext interaction)
  {
    // Go through all of the last 100 messages with an embed, excluding the bot's own messages.
    IMessage[] messages = (await interaction.Channel.GetMessagesAsync(100).FlattenAsync()).ToArray();
    foreach (IEmbed embed in messages.Where(x => x.Embeds.Count > 0 && x.Author.Id != interaction.Client.CurrentUser.Id).Select(x => x.Embeds.First()))
    {
      // Find a beatmap URL in the author URL or normal URL of the embed.
      string[] urls = { embed.Author?.Url ?? "", embed.Url ?? "" };
      string? beatmapUrl = urls.FirstOrDefault(x => x.StartsWith("https://osu.ppy.sh/b/"));
      if (beatmapUrl is null || !int.TryParse(beatmapUrl.Split('/').Last(), out int beatmapId))
        continue;

      // Build a string from various embed components to regex-search for score information.
      string? scoreInfo = $"""
                          {embed.Author}
                          {embed.Description}
                          {string.Join("\n", embed.Fields.Select(x => $"{x.Name}\n{x.Value}"))}
                          """.Replace("**", ""); // Ignore bold text

      // Try to find hits in the format of "[300/100/50/miss]" (owo) or "{300/100/50/miss}" (bathbot). For that, brackets are unified for the regex.
      Match match = Regex.Match(scoreInfo.Replace("{", "[").Replace("}", "]"), "\\[\\d+\\/(\\d+)\\/(\\d+)\\/(\\d+)\\]");
      OsuScoreStatistics? statistics = match.Success
        ? new(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value), null, null)
        : null;

      // Try to find a combo in the format of "x<number>/<number>" (owo) or "<number>x/<number>x" (bathbot).
      match = Regex.Match(scoreInfo, "x(\\d+)\\/\\d+|(\\d+)x\\/\\d+x");
      int? combo = match.Success ? int.Parse(match.Groups[2].Value != "" ? match.Groups[2].Value : match.Groups[1].Value) : null;

      // Try to find the mods in the format of "+<mod1><mod2...>". Potentially attached mod settings (eg. "(1.3x)") are also allowed.
      match = Regex.Match(scoreInfo, "(?<=\\+)[^\\s]+");
      string? mods = match.Success ? match.Value : null;

      // Determine whether the embed is from owo bot by checking whether the beatmap URL origins from the author URL.
      // owo needs some special handling due to it's poor Lazer support. Specifically, classic mod needs to be added.
      if (urls.ToList().IndexOf(beatmapUrl) == 0 && mods is not null)
        mods += "CL";

      // Return the embed score info with all found values.
      return new EmbedScoreInfo(beatmapId, combo, statistics, mods);
    }

    return null;
  }
}

/// <summary>
/// Represents a score parsed from a Discord embed from another osu!-related Discord bot.
/// </summary>
/// <param name="BeatmapId">The ID of the beatmap.</param>
/// <param name="Combo">The amount of combo in the score.</param>
/// <param name="Statistics">The hit statistics in the score.</param>
/// <param name="Mods">The mods applied to the score.</param>
public record EmbedScoreInfo(int BeatmapId, int? Combo = null, OsuScoreStatistics? Statistics = null, string? Mods = null);