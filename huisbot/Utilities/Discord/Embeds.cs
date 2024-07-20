using Discord;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;
using Emoji = huisbot.Utilities.Discord.Emoji;
using DEmoji = Discord.Emoji;

namespace huisbot.Utilities.Discord;

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
  /// Returns an embed notifying the user that they lack Onion permissions.
  /// </summary>
  public static Embed NotOnion => BaseEmbed
    .WithColor(Color.Red)
    .WithTitle("Insufficient permissions.")
    .WithDescription("You need **Onion** permissions in order to access this rework.\nYou can apply for the Onion role here:\n[PP Discord](https://discord.gg/aqPCnXu) • <#1020389783110955008>")
    .Build();

  /// <summary>
  /// Returns an embed notifying the user that they lack PP Team permissions.
  /// </summary>
  public static Embed NotPPTeam => BaseEmbed
    .WithColor(Color.Red)
    .WithTitle("Insufficient permissions.")
    .WithDescription("You need **PP Team** permissions in order to use this command.")
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

    string github = rework.CommitUrl is "" ? "Source unavailable" : $"[Source]({rework.CommitUrl})";
    embed = embed
      .AddField("Ruleset", rework.RulesetName, true)
      .AddField("Links", $"[Huismetbenen](https://pp.huismetbenen.nl/rankings/info/{rework.Code}) • {github}", true)
      .AddField("Status", rework.ReworkTypeString, true);

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
    // Construct some strings for the embed.
    string total = GetPPDifferenceText(local.OldPP, local.NewPP);
    string aim = GetPPDifferenceText(live.AimPP, local.AimPP);
    string tap = GetPPDifferenceText(live.TapPP, local.TapPP);
    string acc = GetPPDifferenceText(live.AccPP, local.AccPP);
    string fl = GetPPDifferenceText(live.FLPP, local.FLPP);
    string? cognition = local.CognitionPP is null ? null : GetPPDifferenceText(live.CognitionPP ?? 0, local.CognitionPP.Value);
    string osuProfile = $"[osu! profile](https://osu.ppy.sh/u/{local.Id})";
    string huisProfile = $"[Huis Profile](https://pp.huismetbenen.nl/player/{local.Id}/{rework.Code})";
    string huisRework = $"[Rework](https://pp.huismetbenen.nl/rankings/info/{rework.Code})";
    string github = rework.CommitUrl is "" ? "Source unavailable" : $"[Source]({rework.CommitUrl})";

    return BaseEmbed
      .WithColor(new Color(0x58A1FF))
      .WithAuthor($"{local.Name} on {rework.Name}", $"https://a.ppy.sh/{local.Id}", $"https://pp.huismetbenen.nl/player/{local.Id}/{rework.Code}")
      .AddField("PP Comparison (Live → Local)", $"▸ **Total**: {total}\n▸ **Aim**: {aim}\n▸ **Tap**: {tap}\n▸ **Acc**: {acc}\n▸ **FL**: {fl}"
              + (cognition is null ? "" : $"\n▸ **Cog**: {cognition}")
       , true)
      .AddField("Useful Links", $"▸ {osuProfile}\n▸ {huisProfile}\n▸ {huisRework}\n▸ {github}", true)
      .WithFooter($"{BaseEmbed.Footer.Text} • Last Updated", BaseEmbed.Footer.IconUrl)
      .WithTimestamp(local.LastUpdated)
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying info about the bot (version, uptime, api status, ...).
  /// </summary>
  /// <param name="osuV1Available">Bool whether the osu! API v1 is available.</param>
  /// <param name="osuV2Available">Bool whether the osu! API v2 is available.</param>
  /// <param name="huisAvailable">Bool whether the Huis api is available.</param>
  /// <returns>An embed for displaying info about the bot.</returns>
  public static Embed Info(bool osuV1Available, bool osuV2Available, bool huisAvailable) => BaseEmbed
    .WithColor(new Color(0xFFD4A8))
    .WithTitle($"Information about Huisbot {Program.VERSION}")
    .WithDescription("This bot aims to provide interaction with [Huismetbenen](https://pp.huismetbenen.nl/) via Discord and is dedicated to the " +
                     "[Official PP Discord](https://discord.gg/aqPCnXu). If any issues come up, please ping `@minisbett` or send them a DM.")
    .AddField("Uptime", $"{(DateTime.UtcNow - Program.STARTUP_TIME).ToUptimeString()}\n\n[Source](https://github.com/minisbett/huisbot) • " +
                        $"[Add To Your Server](https://discord.com/oauth2/authorize?client_id=1174073630330716210&scope=bot&permissions=277025770560)", true)
    .AddField("API Status", $"osu!api v1 {new DEmoji(osuV1Available ? "✅" : "❌")}\nosu!api v2 {new DEmoji(osuV2Available ? "✅" : "❌")}\n" +
                            $"Huismetbenen {new DEmoji(huisAvailable ? "✅" : "❌")}", true)
    .WithThumbnailUrl("https://cdn.discordapp.com/attachments/1009893434087198720/1174333838579732581/favicon.png")
    .Build();

  /// <summary>
  /// Returns an embed for displaying the score calculation progress based on whether the local and live score have been calculated.
  /// </summary>
  /// <param name="local">Bool whether the local score finished calculating.</param>
  /// <param name="liveOnly">Bool whether only the live score calculation should be displayed.</param>
  /// <returns>An embed for displaying the score calculation progress.</returns>
  public static Embed Calculating(bool local, bool liveOnly) => BaseEmbed
    .WithDescription($"*{(local || liveOnly ? "Calculating live score" : "Calculating local score")}...*\n\n" +
                     $"{(liveOnly ? "" : $"{new DEmoji(local ? "✅" : "⏳")} Local\n")}" +
                     $"{new DEmoji(local ? "⏳" : "🕐")} Live")
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
  public static Embed CalculatedScore(HuisSimulatedScore local, HuisSimulatedScore live, HuisRework rework, OsuBeatmap beatmap, double difficultyRating)
  {
    // Construct some strings for the embed.
    string total = GetPPDifferenceText(live.TotalPP, local.TotalPP);
    string aim = GetPPDifferenceText(live.AimPP, local.AimPP);
    string tap = GetPPDifferenceText(live.TapPP, local.TapPP);
    string acc = GetPPDifferenceText(live.AccPP, local.AccPP);
    string fl = GetPPDifferenceText(live.FLPP, local.FLPP);
    string? cognition = local.CognitionPP is null ? null : GetPPDifferenceText(live.CognitionPP ?? 0, local.CognitionPP.Value);
    string hits = $"{local.Count300} {_emojis["300"]} {local.Count100} {_emojis["100"]} {local.Count50} {_emojis["50"]} {local.Misses} {_emojis["miss"]}";
    string combo = $"{local.MaxCombo}/{beatmap.MaxCombo}x";
    string stats1 = $"{beatmap.CircleCount} {_emojis["circles"]} {beatmap.SliderCount} {_emojis["sliders"]} {beatmap.SpinnerCount} {_emojis["spinners"]}";
    string stats2 = $"CS **{beatmap.GetAdjustedCS(local.Mods):0.#}** AR **{beatmap.GetAdjustedAR(local.Mods):0.#}** " +
                    $"▸ **{beatmap.GetBPM(local.Mods):0.###}** {_emojis["bpm"]}";
    string stats3 = $"OD **{beatmap.GetAdjustedOD(local.Mods):0.#}** HP **{beatmap.GetAdjustedHP(local.Mods):0.#}**";
    string stats4 = $"**{Utils.CalculateEstimatedUR(local.Count300, local.Count100, local.Count50, local.Misses, beatmap.CircleCount,
                                                        beatmap.SliderCount, beatmap.GetAdjustedOD(local.Mods), local.Mods.ClockRate):F2}** eUR";
    string visualizer = $"[map visualizer](https://preview.tryz.id.vn/?b={beatmap.Id})";
    string osu = $"[osu! page](https://osu.ppy.sh/b/{beatmap.Id})";
    string huisRework = $"[Huis Rework](https://pp.huismetbenen.nl/rankings/info/{rework.Code})";
    string github = rework.CommitUrl is "" ? "Source unavailable" : $"[Source]({rework.CommitUrl})";

    return BaseEmbed
      .WithColor(new Color(0x4061E9))
      .WithTitle($"{beatmap.Artist} - {beatmap.Title} [{beatmap.Version}]{local.Mods.PlusString} ({difficultyRating:N2}→{local.NewDifficultyRating}★)")
      .AddField("PP Comparison (Live → Local)", $"▸ **PP**: {total}\n▸ **Aim**: {aim}\n▸ **Tap**: {tap}\n▸ **Acc**: {acc}\n▸ **FL**: {fl}"
              + (cognition is null ? "" : $"\n▸ **Cog**: {cognition}")
              + $"\n{visualizer} • {osu}\n{huisRework} • {github}", true)
      .AddField("Score Info", $"▸ {local.Accuracy:N2}% ▸ {combo}\n▸ {hits}\n▸ {stats1}\n▸ {stats2}\n▸ {stats3}\n▸ {stats4}", true)
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
  public static Embed BeatmapAliases(BeatmapAlias[] aliases)
  {
    // Sort the aliases by alphabetical order.
    aliases = aliases.OrderBy(x => x.Alias).ToArray();

    // Build the alias string.
    string aliasesStr = "*There are no beatmap aliases. You can add some via `/alias beatmap add`.*";
    if (aliases.Length > 0)
    {
      aliasesStr = "";
      foreach (IGrouping<long, BeatmapAlias> group in aliases.GroupBy(x => x.BeatmapId))
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
  public static Embed ScoreAliases(ScoreAlias[] aliases)
  {
    // Sort the aliases by alphabetical order.
    aliases = aliases.OrderBy(x => x.Alias).ToArray();

    // Build the alias string.
    string aliasesStr = "*There are no score aliases. You can add some via `/alias score add`.*";
    if (aliases.Length > 0)
    {
      aliasesStr = "";
      foreach (IGrouping<long, ScoreAlias> group in aliases.GroupBy(x => x.ScoreId))
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
  /// <param name="sort">The sort option.</param>
  /// <param name="page">The page being displayed.</param>
  /// <returns>An embed for displaying the score rankings.</returns>
  public static Embed ScoreRankings(HuisScore[] allScores, HuisRework rework, Sort sort, int page)
  {
    // Generate the embed description.
    string github = rework.CommitUrl is "" ? "Source unavailable" : $"[Source]({rework.CommitUrl})";
    List<string> description = new List<string>()
    {
      $"*{rework.Name}*",
      $"[Huis Rework](https://pp.huismetbenen.nl/rankings/info/{rework.Code}) •  {github}",
      ""
    };

    int offset = (page - 1) * 10;
    foreach (HuisScore score in allScores.Skip((page - 1) * 10).Take(10))
    {
      // Trim the version if title + version is too long. If it's still too long, trim title as well.
      string title = score.Title ?? "";
      string version = score.Version ?? "";
      if ($"{title} [{version}]".Length > 60 && version.Length > 27)
        version = $"{version.Substring(0, 27)}...";
      if ($"{title} [{version}]".Length > 60 && title.Length > 27)
        title = $"{title.Substring(0, 27)}...";

      // Add the info to the description lines.
      description.Add($"**#{++offset}** [{score.Username}](https://osu.ppy.sh/u/{score.UserId}) on [{title} [{version}]{score.Mods.PlusString}]" +
                      $"(https://osu.ppy.sh/b/{score.BeatmapId})");
      description.Add($"▸ {GetPPDifferenceText(score.LivePP, score.LocalPP)} ▸ {score.Accuracy:N2}% {score.MaxCombo}x " +
                      $"▸ {score.Count100} {_emojis["100"]} {score.Count50} {_emojis["50"]} {score.Misses} {_emojis["miss"]}");
    }

    // Add hyperlinks to useful urls.
    description.Add($"\n*Displaying scores {page * 10 - 9}-{page * 10} of {allScores.Length} on page {page} of " +
                    $"{Math.Ceiling(allScores.Length / 10d)}.*");

    return BaseEmbed
      .WithTitle($"Score Rankings ({sort.DisplayName})")
      .WithDescription(string.Join("\n", description))
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the top plays of the specified player in the specified rework.
  /// </summary>
  /// <param name="user">The player.</param>
  /// <param name="rawScores">All top plays in their original order. Used to display placement differences.</param>
  /// <param name="sortedScores">All top plays in their sorted order.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="page">The sorting for the scores.</param>
  /// <param name="page">The page being displayed.</param>
  /// <returns>An embed for displaying the top plays.</returns>
  public static Embed TopPlays(OsuUser user, HuisScore[] rawScores, HuisScore[] sortedScores, HuisRework rework, Sort sort, int page)
  {
    // Generate the embed description.
    string github = rework.CommitUrl is "" ? "Source unavailable" : $"[Source]({rework.CommitUrl})";
    List<string> description = new List<string>()
    {
      $"*{rework.Name}*",
      $"[osu! profile](https://osu.ppy.sh/u/{user.Id}) • [Huis Profile](https://pp.huismetbenen.nl/player/{user.Id}/{rework.Code}) • " +
      $"[Huis Rework](https://pp.huismetbenen.nl/rankings/info/{rework.Code}) • {github}",
      ""
    };

    // Go through all scores and populate the description.
    foreach (HuisScore score in sortedScores.Skip((page - 1) * 10).Take(10).ToArray())
    {
      // Trim the version if title + version is too long. If it's still too long, trim title as well.
      string title = score.Title ?? "";
      string version = score.Version ?? "";
      if ($"{title} [{version}]".Length > 80 && version.Length > 37)
        version = $"{version.Substring(0, 37)}...";
      if ($"{title} [{version}]".Length > 80 && title.Length > 37)
        title = $"{title.Substring(0, 37)}...";

      // Get the placement of each score, as well as the difference.
      int placement = rawScores.ToList().IndexOf(score) + 1;
      int placementDiff = rawScores.OrderByDescending(x => x.LivePP).ToList().IndexOf(score) + 1 - placement;
      string placementStr = $"**#{placement}**";
      if (placementDiff != 0)
        placementStr += $" ({placementDiff:+#;-#;0})";

      // Add the info to the description lines.
      description.Add($"{placementStr} [{title} [{version}]{score.Mods.PlusString}](https://osu.ppy.sh/b/{score.BeatmapId})");
      description.Add($"▸ {GetPPDifferenceText(score.LivePP, score.LocalPP)} ▸ {score.Accuracy:N2}% {score.MaxCombo}x " +
                      $"▸ {score.Count100} {_emojis["100"]} {score.Count50} {_emojis["50"]} {score.Misses} {_emojis["miss"]}");
    }

    // Add hyperlinks to useful urls.
    description.Add($"\n*Displaying scores {page * 10 - 9}-{page * 10} of {rawScores.Length} on page {page} of {Math.Ceiling(rawScores.Length / 10d)}.*");

    return BaseEmbed
      .WithTitle($"Top Plays of {user.Name} ({sort.DisplayName})")
      .WithDescription(string.Join("\n", description))
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the player rankings in the specified rework with the specified players.
  /// </summary>
  /// <param name="allPlayers">All players, including the ones to display.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="sort">The sort option.</param>
  /// <param name="page">The page being displayed.</param>
  /// <returns>An embed for displaying the player rankings.</returns>
  public static Embed PlayerRankings(HuisPlayer[] allPlayers, HuisRework rework, Sort sort, int page)
  {
    // Get the players to be displayed.
    HuisPlayer[] players = allPlayers.Skip((page - 1) * 20).Take(20).ToArray();

    // Generate the embed description.
    string github = rework.CommitUrl is "" ? "Source unavailable" : $"[Source]({rework.CommitUrl})";
    List<string> description = new List<string>()
    {
      $"[Huis Rework](https://pp.huismetbenen.nl/rankings/info/{rework.Code}) • {github}",
      ""
    };
    foreach (HuisPlayer player in players)
      // Add the info to the description lines.
      description.Add($"**#{player.Rank?.ToString() ?? "-"}** [{player.Name}](https://osu.ppy.sh/u/{player.Id}) {GetPPDifferenceText(player.OldPP, player.NewPP)} ▸ " +
                      $"[Huis Profile](https://pp.huismetbenen.nl/player{player.Id}/{rework.Code})");

    description.Add($"\n*Displaying players {page * 20 - 19}-{page * 20} on page {page} of {Math.Ceiling(allPlayers.Length / 20d)}.*");

    return BaseEmbed
      .WithTitle($"Player Rankings ({sort.DisplayName})")
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
  /// Returns an embed for displaying the feedback of a user.
  /// </summary>
  /// <param name="user">The Discord user submitting the feedback.</param>
  /// <param name="rework">The name of the rework.</param>
  /// <param name="text">The text body of the feedback.</param>
  /// <returns>An embed for displaying the feedback of a user.</returns>
  public static Embed Feedback(IUser user, string rework, string text) => BaseEmbed
    .WithColor(new Color(0x4287f5))
    .WithAuthor($"{user.Username} ({user.Id})", user.GetAvatarUrl())
    .WithTitle(rework)
    .WithDescription(text)
    .Build();

  /// <summary>
  /// Returns a string representing the difference between two PP values, including the old and new PP values.
  /// </summary>
  /// <param name="oldPP">The old PP.</param>
  /// <param name="newPP">The new PP.</param>
  /// <returns>A string representing the difference between two PP values.</returns>
  private static string GetPPDifferenceText(double oldPP, double newPP)
  {
    // Round the PP values if they're above 1000, as that's irrelevant info and hurts the display flexibility.
    if (oldPP >= 1000)
      oldPP = Math.Round(oldPP);
    if (newPP >= 1000)
      newPP = Math.Round(newPP);

    // Calculate the difference between the two PP values. If it's less than 0.01, the PP values are the same.
    double difference = newPP - oldPP;
    if (Math.Abs(difference) < 0.01)
      return $"**{newPP:0.##}pp**";

    // Otherwise return the difference string.
    return $"{oldPP:0.##} → **{newPP:0.##}pp** *({difference:+#,##0.##;-#,##0.##}pp)*";
  }

  /// <summary>
  /// A dictionary with identifiers for emojis and their corresponding <see cref="Emoji"/> object.
  /// </summary>
  private static readonly Dictionary<string, Emoji> _emojis = new Dictionary<string, Emoji>()
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