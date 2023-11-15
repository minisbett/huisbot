using Discord;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Utils.Extensions;
using System.Text.RegularExpressions;
using System.Web;
using Emoji = huisbot.Models.Utility.Emoji;

namespace huisbot;

/// <summary>
/// Provides embeds for the application.
/// </summary>
internal static class Embeds
{
  /// <summary>
  /// Returns a base embed all other embeds should be based on.
  /// </summary>
  private static EmbedBuilder BaseEmbed => new EmbedBuilder()
    .WithColor(new Color(0xF1C40F))
    .WithFooter($"huisbot v{Program.VERSION} by minisbett", "https://pp.huismetbenen.nl/favicon.ico")
    .WithCurrentTimestamp();

  /// <summary>
  /// Returns an error embed for displaying an internal error message.
  /// </summary>
  /// <param name="message">The error message.</param>
  /// <returns>An embed for displaying an internal error message.</returns>
  public static Embed InternalError(string message) => BaseEmbed
    .WithColor(Color.Red)
    .WithTitle("An internal error occured.")
    .WithDescription(message)
    .Build();

  /// <summary>
  /// Returns an error embed for displaying an error message.
  /// </summary>
  /// <param name="message">The error message.</param>
  /// <returns>An embed for displaying an error message.</returns>
  public static Embed Error(string message) => BaseEmbed
    .WithColor(Color.Red)
    .WithDescription(message)
    .Build();

  /// <summary>
  /// Returns an error embed for displaying a neutral message.
  /// </summary>
  /// <param name="message">The neutral message.</param>
  /// <returns>An embed for displaying a neutral message.</returns>
  public static Embed Neutral(string message) => BaseEmbed
    .WithDescription(message)
    .Build();

  /// <summary>
  /// Returns an embed with the specified rework.
  /// </summary>
  /// <param name="rework">The rework to display.</param>
  /// <returns>The embed for displaying the specified rework.</returns>
  public static Embed Rework(HuisRework rework) => BaseEmbed
    .WithTitle($"{rework.Id} {rework.Name} ({rework.Code})")
    .WithUrl($"https://pp.huismetbenen.nl/rankings/info/{rework.Code}")
    .AddField("Description", (rework.Description ?? "") == "" ? "*No description available.*" : rework.Description)
    .AddField("Ruleset", rework.GetReadableRuleset(), true)
    .AddField("Links", $"[Huismetbenen](https://pp.huismetbenen.nl/rankings/info/{rework.Code}) • [Source Code]({rework.GetCommitUrl()})", true)
    .AddField("Status", rework.GetReadableReworkType(), true)
    .Build();

  /// <summary>
  /// Returns an embed for displaying the specified player in the specified rework.
  /// </summary>
  /// <param name="player">The player to display.</param>
  /// <param name="rework">The rework.</param>
  /// <returns>An embed for displaying the specified player in the specified rework.</returns>
  public static Embed Player(HuisPlayer player, HuisRework rework)
  {
    string pp = $"{player.OldPP:N2} → **{player.NewPP:N2}pp** *({player.NewPP - player.OldPP:+#,##0.00;-#,##0.00}pp)*\n(inclusive {player.BonusPP:N2}pp bonus pp)";
    string weightedpp1 = $"**Acc** {player.WeightedAccPP:N2}pp | **Aim** {player.WeightedAimPP:N2}pp";
    string weightedpp2 = $"**Tap** {player.WeightedTapPP:N2}pp | **Accuracy** {player.WeightedAccPP:N2}pp";
    string huisRework = $"[Rework](https://pp.huismetbenen.nl/rankings/info/{rework.Code})";
    string huisProfile = $"[Huis Profile](https://pp.huismetbenen.nl/player/{player.Id}/{rework.Code})";

    return BaseEmbed
      .WithColor(new Color(0x58A1FF))
      .WithAuthor($"{player.Name} on {rework.Name}", $"https://a.ppy.sh/{player.Id}", $"https://pp.huismetbenen.nl/player/{player.Id}/{rework.Code}")
      .AddField("Comparison of Total PP", pp, true)
      .AddField("Weighted PP", $"{weightedpp1}\n{weightedpp2}", true)
      .AddField("**Useful Links**", $"[osu! profile](https://osu.ppy.sh/u/{player.Id}) • {huisRework} • {huisProfile} • [Source Code]({rework.GetCommitUrl()})")
      .WithFooter($"{BaseEmbed.Footer.Text} • Last Updated", BaseEmbed.Footer.IconUrl)
      .WithTimestamp(player.LastUpdated)
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying info about the bot (version, uptime, api status, ...).
  /// </summary>
  /// <param name="osuAvailable">Bool whether the osu! api is available.</param>
  /// <param name="huisAvailable">Bool whether the Huis api is available.</param>
  /// <returns>An embed for displaying info about the bot.</returns>
  public static Embed Info(bool osuAvailable, bool huisAvailable) => BaseEmbed
    .WithColor(new Color(0xFFD4A8))
    .WithTitle($"Information about Huisbot {Program.VERSION}")
    .WithDescription("This bot aims to provide interaction with [Huismetbenen](https://pp.huismetbenen.nl/) via Discord and is exclusive to the " +
                     "[Official PP Discord](https://discord.gg/aqPCnXu). If any issues come up, please ping `@minisbett` here or send them a DM.")
    .AddField("Uptime", $"{(DateTime.UtcNow - Program.STARTUP_TIME).ToUptimeString()}\n\n[Source Code](https://github.com/minisbett/huisbot)", true)
    .AddField("API Status", $"osu! {new Discord.Emoji(osuAvailable ? "✅" : "❌")}\nHuismetbenen {new Discord.Emoji(osuAvailable ? "✅" : "❌")}", true)
    .WithThumbnailUrl("https://cdn.discordapp.com/attachments/1009893434087198720/1174333838579732581/favicon.png")
    .Build();

  /// <summary>
  /// Returns an embed for displaying the score calculation progress based on whether the local and live score have been calculated.
  /// </summary>
  /// <param name="local">Bool whether the local score finished calculating.</param>
  /// <param name="live">Bool whether the live score finished calculating.</param>
  /// <returns>An embed for displaying the score calculation progress.</returns>
  public static Embed Calculating(bool local, bool live) => BaseEmbed
    .WithDescription($"*{(local ? live ? "Finalizing" : "Calculating live score" : "Calculating local score")}...*\n\n" +
                     $"{new Discord.Emoji(local ? "✅" : "⏳")} Local\n{new Discord.Emoji(local ? live ? "✅" : "⏳" : "🕐")} Live")
    .Build();

  /// <summary>
  /// Returns an embed for displaying a calculated score and it's difference to the current live state.
  /// </summary>
  /// <param name="local">The local score in the rework.</param>
  /// <param name="live">The score on the live servers.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="beatmap">The beatmap.</param>
  /// <returns>An embed for displaying a calculated score</returns>
  public static Embed CalculatedScore(HuisCalculationResult local, HuisCalculationResult live, HuisRework rework, OsuBeatmap beatmap)
  {
    // Split the map name into it's components using regex.
    Match match = Regex.Match(local.MapName ?? "", @"^(\d+) - (.+) - (.+) \((.+)\) \[(.+)\]$");
    string beatmapId = match.Groups[1].Value;
    string artist = match.Groups[2].Value;
    string title = match.Groups[3].Value;
    string creator = match.Groups[4].Value;
    string version = match.Groups[5].Value;

    // Construct some strings for the embed.
    string total = $"{live.TotalPP:N2} → **{local.TotalPP:N2}pp** *({local.TotalPP - live.TotalPP:+#,##0.00;-#,##0.00}pp)*";
    string aim = live.AimPP == local.AimPP ? $"{local.AimPP:N2}pp" : $"{live.AimPP:N2} → **{local.AimPP:N2}pp** *({local.AimPP - live.AimPP:+#,##0.00;-#,##0.00}pp)*";
    string acc = live.AccPP == local.AccPP ? $"{local.AccPP:N2}pp" : $"{live.AccPP:N2} → **{local.AccPP:N2}pp** *({local.AccPP - live.AccPP:+#,##0.00;-#,##0.00}pp)*";
    string tap = live.TapPP == local.TapPP ? $"{local.TapPP:N2}pp" : $"{live.TapPP:N2} → **{local.TapPP:N2}pp** *({local.TapPP - live.TapPP:+#,##0.00;-#,##0.00}pp)*";
    string fl = live.FLPP == local.FLPP ? $"{local.FLPP:N2}pp" : $"~~{live.FLPP:N2}~~ {local.FLPP:N2}pp *({local.FLPP - live.FLPP:+#,##0.00;-#,##0.00}pp)*";
    string hits = $"{local.Count300} {_emojis["300"]} {local.Count100} {_emojis["100"]} {local.Count50} {_emojis["50"]} {local.Misses} {_emojis["miss"]}";
    string combo = $"{local.MaxCombo}/{beatmap.MaxCombo}x";
    string mods = local.Mods.Replace("CL", "") == "" ? "" : $"+{local.Mods.Replace(", ", "").Replace("CL", "")}";
    string links = $"[map visualizer](https://osu.direct/preview?b={beatmapId}) • [osu! page](https://osu.ppy.sh/b/{beatmapId}) • " +
                   $"[Rework](https://pp.huismetbenen.nl/rankings/info/{rework.Code})";

    return BaseEmbed
      .WithColor(new Color(0x4061E9))
      .WithTitle($"{artist} - {title} [{version}] {mods}")
      .AddField("PP Comparison (Live → Local)", $"▸ **Total**: {total}\n▸ **Aim**: {aim}\n▸ **Tap**: {aim}\n▸ **Acc**: {aim}\n▸ **FL**: {fl}\n{links}", true)
      .AddField("Score Info", $"▸ {local.Accuracy:N2}% ▸ {combo}\n{hits}", true)
      .WithUrl($"https://osu.ppy.sh/b/{beatmapId}")
      .WithImageUrl($"https://assets.ppy.sh/beatmaps/{beatmap.BeatmapSetId}/covers/slimcover@2x.jpg")
      .WithFooter($"{rework.Name} • {BaseEmbed.Footer.Text}", BaseEmbed.Footer.IconUrl)
    .Build();
  }

  /// <summary>
  /// A dictionary with identifiers for emojis and their corresponding <see cref="Emoji"/> object.
  /// </summary>
  private static Dictionary<string, Emoji> _emojis = new Dictionary<string, Emoji>()
  {
    { "XH", new Emoji("rankSSH", 1159888184600170627) },
    { "X", new Emoji("rankSS", 1159888182075207740) },
    { "SH", new Emoji("rankSH", 1159888343245537300) },
    { "S", new Emoji("rankS", 1159888340536012921) },
    { "A", new Emoji("rankA", 1159888148080361592) },
    { "B", new Emoji("rankB", 1159888151771369562) },
    { "C", new Emoji("rankC", 1159888154891919502) },
    { "D", new Emoji("rankD", 1159888158150893678) },
    { "F", new Emoji("rankF", 1159888321342865538) },
    { "300", new Emoji("300", 1159888146448797786) },
    { "100", new Emoji("100", 1159888144406171719) },
    { "50", new Emoji("50", 1159888143282094221) },
    { "miss", new Emoji("miss", 1159888326698995842)},
    { "loved", new Emoji("loved", 1159888325491036311) },
    { "qualified", new Emoji("approved", 1159888150542418031) },
    { "approved", new Emoji("approved", 1159888150542418031) },
    { "ranked", new Emoji("ranked", 1159888338199773339) },
    { "length", new Emoji("length", 1159888322873786399) },
    { "bpm", new Emoji("length", 1159888153000280074) },
    { "circles", new Emoji("circles", 1159888155902758953) },
    { "sliders", new Emoji("sliders", 1159888389902970890) },
    { "spinners", new Emoji("spinners", 1159888345250414723) },
    { "osu", new Emoji("std", 1159888333044981913) },
    { "taiko", new Emoji("taiko", 1159888334492029038) },
    { "fruits", new Emoji("fruits", 1159888328984903700) },
    { "mania", new Emoji("mania", 1159888330637463623) },
  };
}