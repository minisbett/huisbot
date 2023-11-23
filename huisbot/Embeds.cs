using Discord;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Utility;
using huisbot.Utils.Extensions;
using ScottPlot.Statistics;
using System.ComponentModel.Design;
using System.Numerics;
using System.Text.RegularExpressions;
using static System.Formats.Asn1.AsnWriter;
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
  /// Returns an error embed for displaying a success message.
  /// </summary>
  /// <param name="message">The success message.</param>
  /// <returns>An embed for displaying a success message.</returns>
  public static Embed Success(string message) => BaseEmbed
    .WithColor(Color.Green)
    .WithDescription(message)
    .Build();

  /// <summary>
  /// Returns an embed with the specified rework.
  /// </summary>
  /// <param name="rework">The rework to display.</param>
  /// <returns>The embed for displaying the specified rework.</returns>
  public static Embed Rework(HuisRework rework)
  {
    // Divide the description in multiple parts due to the 1024 character limit.
    string[] descriptionParts = (rework.Description ?? "") != "" ? rework.Description!.Split("\n\n") : new string[] { "*No description available.*" };

    EmbedBuilder embed = BaseEmbed
    .WithTitle($"{rework.Id} {rework.Name} ({rework.Code})")
    .WithUrl($"https://pp.huismetbenen.nl/rankings/info/{rework.Code}")
    .AddField("Description", descriptionParts[0]);

    // Add the description parts to the embed.
    foreach (string part in descriptionParts.Skip(1))
      embed = embed.AddField("\u200B", part);

    embed = embed
      .AddField("Ruleset", rework.GetReadableRuleset(), true)
      .AddField("Links", $"[Huismetbenen](https://pp.huismetbenen.nl/rankings/info/{rework.Code}) • [Source]({rework.GetCommitUrl()})", true)
      .AddField("Status", rework.GetReworkStatus(), true);

    return embed.Build();
  }

  /// <summary>
  /// Returns an embed for displaying the specified player in the specified rework.
  /// </summary>
  /// <param name="local">The player to display.</param>
  /// <param name="rework">The rework.</param>
  /// <returns>An embed for displaying the specified player in the specified rework.</returns>
  public static Embed Player(HuisPlayer local, HuisPlayer live, HuisRework rework)
  {
    string total = Math.Abs(local.NewPP - local.OldPP) < 0.01 ? $"**{local.NewPP:N2}pp**" : $"{live.OldPP:N2} → **{local.NewPP:N2}pp** *({local.NewPP - live.OldPP:+#,##0.00;-#,##0.00}pp)*";
    string aim = Math.Abs(live.AimPP - local.AimPP) < 0.01 ? $"**{local.AimPP:N2}pp**" : $"{live.AimPP:N2} → **{local.AimPP:N2}pp** *({local.AimPP - live.AimPP:+#,##0.00;-#,##0.00}pp)*";
    string tap = Math.Abs(live.TapPP - local.TapPP) < 0.01 ? $"**{local.TapPP:N2}pp**" : $"{live.TapPP:N2} → **{local.TapPP:N2}pp** *({local.TapPP - live.TapPP:+#,##0.00;-#,##0.00}pp)*";
    string acc = Math.Abs(live.AccPP - local.AccPP) < 0.01 ? $"**{local.AccPP:N2}pp**" : $"{live.AccPP:N2} → **{local.AccPP:N2}pp** *({local.AccPP - live.AccPP:+#,##0.00;-#,##0.00}pp)*";
    string fl = Math.Abs(live.FLPP - local.FLPP) < 0.01 ? $"{local.FLPP:N2}pp" : $"~~{live.FLPP:N2}~~ {local.FLPP:N2}pp *({local.FLPP - live.FLPP:+#,##0.00;-#,##0.00}pp)*";
    string osuProfile = $"[osu! profile](https://osu.ppy.sh/u/{local.Id})";
    string huisProfile = $"[Huis Profile](https://pp.huismetbenen.nl/player/{local.Id}/{rework.Code})";
    string huisRework = $"[Rework](https://pp.huismetbenen.nl/rankings/info/{rework.Code})";
    string github = $"[Source]({rework.GetCommitUrl()})";

    return BaseEmbed
      .WithColor(new Color(0x58A1FF))
      .WithAuthor($"{local.Name} on {rework.Name}", $"https://a.ppy.sh/{local.Id}", $"https://pp.huismetbenen.nl/player/{local.Id}/{rework.Code}")
      .AddField("PP Comparison (Live → Local)", $"▸ **Total**: {total}\n▸ **Aim**: {aim}\n▸ **Tap**: {tap}\n▸ **Acc**: {acc}\n▸ **FL**: {fl}", true)
      .AddField("Useful Links", $"▸ {osuProfile}\n▸ {huisProfile}\n▸ {huisRework}\n▸ {github}", true)
      .WithFooter($"{BaseEmbed.Footer.Text} • Last Updated", BaseEmbed.Footer.IconUrl)
      .WithTimestamp(local.LastUpdated)
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying info about the bot (version, uptime, api status, ...).
  /// </summary>
  /// <param name="osuAvailable">Bool whether the osu! api is available.</param>
  /// <param name="huisAvailable">Bool whether the Huis api is available.</param>
  /// <returns>An embed for displaying info about the bot.</returns>
  public static Embed Info(bool osuV1Available, bool osuV2Available, bool huisAvailable) => BaseEmbed
    .WithColor(new Color(0xFFD4A8))
    .WithTitle($"Information about Huisbot {Program.VERSION}")
    .WithDescription("This bot aims to provide interaction with [Huismetbenen](https://pp.huismetbenen.nl/) via Discord and is exclusive to the " +
                     "[Official PP Discord](https://discord.gg/aqPCnXu). If any issues come up, please ping `@minisbett` here or send them a DM.")
    .AddField("Uptime", $"{(DateTime.UtcNow - Program.STARTUP_TIME).ToUptimeString()}\n\n[Source](https://github.com/minisbett/huisbot)", true)
    .AddField("API Status", $"osu!api v1 {new Discord.Emoji(osuV1Available ? "✅" : "❌")}\nosu!api v2 {new Discord.Emoji(osuV2Available ? "✅" : "❌")}\n" +
                            $"Huismetbenen {new Discord.Emoji(huisAvailable ? "✅" : "❌")}", true)
    .WithThumbnailUrl("https://cdn.discordapp.com/attachments/1009893434087198720/1174333838579732581/favicon.png")
    .Build();

  /// <summary>
  /// Returns an embed for displaying the score calculation progress based on whether the local and live score have been calculated.
  /// </summary>
  /// <param name="local">Bool whether the local score finished calculating.</param>
  /// <param name="live">Bool whether the live score finished calculating.</param>
  /// <param name="liveOnly">Bool whether only the live score calculation should be displayed.</param>
  /// <returns>An embed for displaying the score calculation progress.</returns>
  public static Embed Calculating(bool local, bool liveOnly) => BaseEmbed
    .WithDescription($"*{(local || liveOnly ? "Calculating live score" : "Calculating local score")}...*\n\n" +
                     $"{(liveOnly ? "" : $"{new Discord.Emoji(local ? "✅" : "⏳")} Local\n")}" +
                     $"{new Discord.Emoji(local ? "⏳" : "🕐")} Live")
    .Build();

  /// <summary>
  /// Returns an embed for displaying a calculated score and it's difference to the current live state.
  /// </summary>
  /// <param name="local">The local score in the rework.</param>
  /// <param name="live">The score on the live servers.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="beatmap">The beatmap.</param>
  /// <param name="difficultyRating">The difficulty rating of the score.</param>
  /// <returns>An embed for displaying a calculated score</returns>
  public static Embed CalculatedScore(HuisCalculatedScore local, HuisCalculatedScore live, HuisRework rework, OsuBeatmap beatmap, double difficultyRating)
  {
    // Construct some strings for the embed.
    string total = Math.Abs(local.TotalPP - live.TotalPP) < 0.01 ? $"**{local.TotalPP:N2}pp**" : $"{live.TotalPP:N2} → **{local.TotalPP:N2}pp** *({local.TotalPP - live.TotalPP:+#,##0.00;-#,##0.00}pp)*";
    string aim = Math.Abs(live.AimPP - local.AimPP) < 0.01 ? $"**{local.AimPP:N2}pp**" : $"{live.AimPP:N2} → **{local.AimPP:N2}pp** *({local.AimPP - live.AimPP:+#,##0.00;-#,##0.00}pp)*";
    string tap = Math.Abs(live.TapPP - local.TapPP) < 0.01 ? $"**{local.TapPP:N2}pp**" : $"{live.TapPP:N2} → **{local.TapPP:N2}pp** *({local.TapPP - live.TapPP:+#,##0.00;-#,##0.00}pp)*";
    string acc = Math.Abs(live.AccPP - local.AccPP) < 0.01 ? $"**{local.AccPP:N2}pp**" : $"{live.AccPP:N2} → **{local.AccPP:N2}pp** *({local.AccPP - live.AccPP:+#,##0.00;-#,##0.00}pp)*";
    string fl = Math.Abs(live.FLPP - local.FLPP) < 0.01 ? $"{local.FLPP:N2}pp" : $"~~{live.FLPP:N2}~~ {local.FLPP:N2}pp *({local.FLPP - live.FLPP:+#,##0.00;-#,##0.00}pp)*";
    string hits = $"{local.Count300} {_emojis["300"]} {local.Count100} {_emojis["100"]} {local.Count50} {_emojis["50"]} {local.Misses} {_emojis["miss"]}";
    string combo = $"{local.MaxCombo}/{beatmap.MaxCombo}x";
    string modsStr = local.Mods.Replace(", ", "").Replace("CL", "");
    string mods = modsStr == "" ? "" : $"+{modsStr}";
    string stats1 = $"CS **{beatmap.AdjustedCS(modsStr):0.#}** AR **{beatmap.AdjustedAR(modsStr):0.#}** ▸ **{beatmap.BPM:0.###}** {_emojis["bpm"]}";
    string stats2 = $"OD **{beatmap.AdjustedOD(modsStr):0.#}** HP **{beatmap.AdjustedHP(modsStr):0.#}**";
    string visualizer = $"[map visualizer](https://osu.direct/preview?b={beatmap.Id})";
    string osu = $"[osu! page](https://osu.ppy.sh/b/{beatmap.Id})";
    string huisRework = $"[Huis Rework](https://pp.huismetbenen.nl/rankings/info/{rework.Code})";
    string github = $"[Source]({rework.GetCommitUrl()})";

    return BaseEmbed
      .WithColor(new Color(0x4061E9))
      .WithTitle($"{beatmap.Artist} - {beatmap.Title} [{beatmap.Version}] {mods} [{difficultyRating:N2}★]")
      .AddField("PP Comparison (Live → Local)", $"▸ **Total**: {total}\n▸ **Aim**: {aim}\n▸ **Tap**: {tap}\n▸ **Acc**: {acc}\n▸ **FL**: {fl}\n" +
               $"{visualizer} • {osu} • {huisRework} • {github}", true)
      .AddField("Score Info", $"▸ {local.Accuracy:N2}% ▸ {combo}\n▸ {hits}\n▸ {stats1}\n▸ {stats2}", true)
      .WithUrl($"https://osu.ppy.sh/b/{beatmap.Id}")
      .WithImageUrl($"https://assets.ppy.sh/beatmaps/{beatmap.SetId}/covers/slimcover@2x.jpg")
      .WithFooter($"{rework.Name} • {BaseEmbed.Footer.Text}", BaseEmbed.Footer.IconUrl)
    .Build();
  }

  /// <summary>
  /// Returns an embed for displaying a successful link between a Discord and an osu! account.
  /// </summary>
  /// <param name="user">The osu! user that was linked.</param>
  /// <returns>An embed for displaying a successful link between a Discord and an osu! account.</returns>
  public static Embed LinkSuccessful(OsuUser user) => BaseEmbed
    .WithColor(Color.Green)
    .WithDescription($"Your Discord account was successfully linked to the osu! account `{user.Name}`.")
    .WithThumbnailUrl($"https://a.ppy.sh/{user.Id}")
    .Build();

  /// <summary>
  /// Returns an embed for displaying all beatmap aliases.
  /// </summary>
  /// <param name="aliases">The beatmap aliases.</param>
  /// <returns>An embed for displaying the beatmap aliases.</returns>
  public static Embed BeatmapAliases(IDAlias[] aliases)
  {
    // Sort the aliases by alphabetical order.
    aliases = aliases.OrderBy(x => x.Alias).ToArray();

    // Build the alias string.
    string aliasesStr = "*There are no beatmap aliases. You can add some via `/alias beatmap add`.*";
    if (aliases.Length > 0)
    {
      aliasesStr = "";
      foreach (IGrouping<int, IDAlias> group in aliases.GroupBy(x => x.Id))
        aliasesStr += $"[{group.First().DisplayName}](https://osu.ppy.sh/b/{group.Key})\n▸ {string.Join(", ", group.Select(j => $"`{j.Alias}`"))}\n\n";
    }

    return BaseEmbed
      .WithTitle("List of all beatmap aliases")
      .WithDescription($"*These aliases can be in place of where you'd specify a beatmap ID in order to access those beatmaps more easily.*\n\n{aliasesStr}")
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying all score aliases.
  /// </summary>
  /// <param name="aliases">The score aliases.</param>
  /// <returns>An embed for displaying the score aliases.</returns>
  public static Embed ScoreAliases(IDAlias[] aliases)
  {
    // Sort the aliases by alphabetical order.
    aliases = aliases.OrderBy(x => x.Alias).ToArray();

    // Build the alias string.
    string aliasesStr = "*There are no score aliases. You can add some via `/alias score add`.*";
    if (aliases.Length > 0)
    {
      aliasesStr = "";
      foreach (IGrouping<long, IDAlias> group in aliases.GroupBy(x => x.Id))
        aliasesStr += $"[{group.First().DisplayName}](https://osu.ppy.sh/scores/osu/{group.Key})\n▸ {string.Join(", ", group.Select(j => $"`{j.Alias}`"))}\n\n";
    }

    return BaseEmbed
      .WithTitle("List of all score aliases")
      .WithDescription($"*These aliases can be in place of where you'd specify a score ID in order to access those scores more easily.*\n\n{aliasesStr}")
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the score rankings in the specified rework with the specified scores.
  /// </summary>
  /// <param name="allScores">All scores, including the ones to display.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="page">The page being displayed.</param>
  /// <returns>An embed for displaying the score rankings.</returns>
  public static Embed ScoreRankings(HuisScore[] allScores, HuisRework rework, int page)
  {
    // Get the scores to be displayed.
    HuisScore[] scores = allScores.Skip((page - 1) * 10).Take(10).ToArray();

    // Generate the embed description.
    List<string> description = new List<string>()
    {
      $"*{rework.Name}*",
      $"[Huis Rework](https://pp.huismetbenen.nl/rankings/info/{rework.Code}) • [Source]({rework.GetCommitUrl()})",
      ""
    };

    int offset = (page - 1) * 10;
    foreach (HuisScore score in scores)
    {
      // Trim the version if title + version is too long. If it's still too long, trim title as well.
      string title = score.Title ?? "";
      string version = score.Version ?? "";
      if ($"{title} [{version}]".Length > 60 && version.Length > 27)
        version = $"{version.Substring(0, 27)}...";
      if ($"{title} [{version}]".Length > 60 && title.Length > 27)
        title = $"{title.Substring(0, 27)}...";

      string pp = score.LivePP == score.LocalPP ? $"**{score.LocalPP:N2}pp**" : $"{score.LivePP:N2} → **{score.LocalPP:N2}pp** *({score.LocalPP - score.LivePP:+#,##0.00;-#,##0.00}pp)*";
      string modsStr = score.Mods.Replace(", ", "").Replace("CL", "");
      string mods = modsStr == "" ? "" : $"+{modsStr}";

      // Add the info to the description lines.
      description.Add($"**#{++offset}** [{score.Username}](https://osu.ppy.sh/u/{score.UserId}) on [{title} [{version}] {mods}]" +
                      $"(https://osu.ppy.sh/b/{score.BeatmapId})");
      description.Add($"▸ {pp} ▸ {score.Accuracy:N2}% {score.MaxCombo}x ▸ {score.Count100} {_emojis["100"]} {score.Count50} {_emojis["50"]} {score.Misses} {_emojis["miss"]}");
    }

    // Add hyperlinks to useful urls.
    description.Add($"\n*Displaying scores {page * 10 - 9}-{page * 10} of {allScores.Length} on page {page} of " +
                    $"{Math.Ceiling(allScores.Length / 10d)}.*");

    return BaseEmbed
      .WithTitle($"Score Rankings on {rework.Name}")
      .WithDescription(string.Join("\n", description))
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the top plays of the specified player in the specified rework.
  /// </summary>
  /// <param name="user">The player.</param>
  /// <param name="allScores">All scores, including the ones to display.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="page">The page being displayed.</param>
  /// <returns>An embed for displaying the top plays.</returns>
  public static Embed TopPlays(OsuUser user, HuisScore[] allScores, HuisRework rework, int page)
  {
    // Get the scores to be displayed.
    HuisScore[] scores = allScores.Skip((page - 1) * 10).Take(10).ToArray();

    // Generate the embed description.
    List<string> description = new List<string>()
    {
      $"*{rework.Name}*",
      $"[Huis Rework](https://pp.huismetbenen.nl/rankings/info/{rework.Code}) • [Source]({rework.GetCommitUrl()})",
      ""
    };

    int offset = (page - 1) * 10;
    foreach (HuisScore score in scores)
    {
      // Trim the version if title + version is too long. If it's still too long, trim title as well.
      string title = score.Title ?? "";
      string version = score.Version ?? "";
      if ($"{title} [{version}]".Length > 80 && version.Length > 37)
        version = $"{version.Substring(0, 37)}...";
      if ($"{title} [{version}]".Length > 80 && title.Length > 37)
        title = $"{title.Substring(0, 37)}...";

      string pp = Math.Abs(score.LocalPP - score.LivePP) < 0.01 ? $"**{score.LocalPP:N2}pp**" : $"{score.LivePP:N2} → **{score.LocalPP:N2}pp** *({score.LocalPP - score.LivePP:+#,##0.00;-#,##0.00}pp)*";
      string modsStr = score.Mods.Replace(", ", "").Replace("CL", "");
      string mods = modsStr == "" ? "" : $"+{modsStr}";

      // Add the info to the description lines.
      description.Add($"**#{++offset}** [{title} [{version}] {mods}](https://osu.ppy.sh/b/{score.BeatmapId})");
      description.Add($"▸ {pp} ▸ {score.Accuracy:N2}% {score.MaxCombo}x ▸ {score.Count100} {_emojis["100"]} {score.Count50} {_emojis["50"]} {score.Misses} {_emojis["miss"]}");
    }

    // Add hyperlinks to useful urls.
    description.Add($"\n*Displaying scores {page * 10 - 9}-{page * 10} of {allScores.Length} on page {page} of " +
                    $"{Math.Ceiling(allScores.Length / 10d)}.*");

    return BaseEmbed
      .WithTitle($"Top Plays of {user.Name}")
      .WithDescription(string.Join("\n", description))
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the player rankings in the specified rework with the specified players.
  /// </summary>
  /// <param name="allPlayers">All players, including the ones to display.</param>
  /// <param name="rework">The rework.</param>
  /// <returns>An embed for displaying the player rankings.</returns>
  public static Embed PlayerRankings(HuisPlayer[] allPlayers, HuisRework rework, int page)
  {
    // Get the players to be displayed.
    HuisPlayer[] players = allPlayers.Skip((page - 1) * 20).Take(20).ToArray();

    // Generate the embed description.
    List<string> description = new List<string>()
    { $"[Huis Rework](https://pp.huismetbenen.nl/rankings/info/{rework.Code}) • [Source]({rework.GetCommitUrl()})\n" };
    foreach (HuisPlayer player in players)
    {
      string pp = Math.Abs(player.NewPP - player.OldPP) < 0.01 ? $"**{player.NewPP:N2}pp**" : $"{player.OldPP:N2} → **{player.NewPP:N2}pp** *({player.NewPP - player.OldPP:+#,##0.00;-#,##0.00}pp)*";

      // Add the info to the description lines.
      description.Add($"**#{player.Rank?.ToString() ?? "-"}** [{player.Name}](https://osu.ppy.sh/u/{player.Id}) {pp} ▸ " +
                      $"[Huis Profile](https://pp.huismetbenen.nl/player{player.Id}/{rework.Code})");
    }

    description.Add($"\n*Displaying players {page * 20 - 19}-{page * 20} on page {page} of {Math.Ceiling(allPlayers.Length / 20d)}.*");

    return BaseEmbed
      .WithTitle($"Player Rankings on {rework.Name}")
      .WithDescription(string.Join("\n", description))
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the effective misscount breakdown of the specified score.
  /// </summary>
  /// <param name="combo">The combo of the score.</param>
  /// <param name="maxCombo">The maximum achievable combo.</param>
  /// <param name="sliderCount">The amount of sliders.</param>
  /// <param name="hits">The amount of 100s and 50s combined.</param>
  /// <param name="misses">The amount of misses.</param>
  /// <param name="cbmc">The combo-based misscount.</param>
  /// <param name="fct">The full-combo threshold.</param>
  /// <param name="emc">The effective misscount.</param>
  /// <returns>An embed for displaying the effective misscount breakdown.</returns>
  public static Embed EffMissCount(int combo, int maxCombo, int sliderCount, int hits, int misses, double cbmc, double fct, double emc) => BaseEmbed
    .WithColor(new Color(0x812E2E))
    .WithTitle("Effective Misscount Breakdown")
    .WithDescription($"▸ {combo}/{maxCombo}x ▸ {hits} {_emojis["100"]}{_emojis["50"]} {misses} {_emojis["miss"]} ▸ {sliderCount} {_emojis["sliders"]}\n" +
                     $"```\n" +
                     $"combo-based misscount | {cbmc.ToString($"N{Math.Max(0, 6 - ((int)cbmc).ToString().Length)}")}\n" +
                     $"full-combo threshold  | {fct.ToString($"N{Math.Max(0, 6 - ((int)fct).ToString().Length)}")}\n" +
                     $"-------------------------------\n" +
                     $"effective misscount   | {emc.ToString($"N{Math.Max(0, 6 - ((int)emc).ToString().Length)}")}\n" +
                     $"```" +
                     $"*The reference code can be found [here](https://github.com/ppy/osu/blob/3d569850b15ad66b3c95e009f173298d65a8e3de/osu.Game.Rulesets.Osu/Difficulty/OsuPerformanceCalculator.cs#L249).*")
    .Build();

  /// <summary>
  /// Returns an embed for displaying the UR estimation breakdown of the specified score.
  /// </summary>
  /// <param name="ur">The estimated UR.</param>
  /// <returns></returns>
  internal static Embed EstimateUR(double hitWindow300, double hitWindow100, double hitWindow50, double? ur) => BaseEmbed
    .WithColor(new Color(0x812E2E))
    .WithTitle("UR estimation Breakdown")
    .WithDescription($"Estimated UR: {ur?.ToString("N2") ?? "null"}\n" +
                     $"*The reference code can be found [here](https://github.com/Fr0stium/osu/blob/e09beebe1de2fc64538606495d8597c36f9e3353/osu.Game.Rulesets.Osu/Difficulty/OsuPerformanceCalculator.cs#L324).*")
    .Build();

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