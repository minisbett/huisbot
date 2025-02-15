using Discord;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;
using DEmoji = Discord.Emoji;

namespace huisbot.Utilities;

/// <summary>
/// Provides embeds for the application.
/// </summary>
internal static class Embeds
{
  /// <summary>
  /// The amount of top plays to display per page.<br/>
  /// <see cref="TopPlays(OsuUser, HuisScore[], HuisScore[], HuisRework, Sort, int)"/><br/>
  /// <see cref="ScoreRankings(HuisScore[], HuisRework, Sort, int)"/>
  /// </summary>
  public const int SCORES_PER_PAGE = 10;

  /// <summary>
  /// The amount of top players to display per page.<br/>
  /// <see cref="PlayerRankings(HuisPlayer[], HuisRework, Sort, int)"/>
  /// </summary>
  public const int PLAYERS_PER_PAGE = 20;

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
    // Parse the rich text description of the rework.
    string description = rework.Description!;
    foreach (string tag in Enumerable.Range(1, 4).SelectMany(x => new string[] { $"<h{x}>", $"</h{x}>" }))
      description = description.Replace(tag, "");
    description = new ReverseMarkdown.Converter().Convert(description);

    // Divide the description in multiple parts due to the 1024 character limit.
    List<string> descriptionParts = description
        .Split("\n\n")
        .SelectMany(section =>
        {
          List<string> result = [];

          // If the section is more than 1024 characters, split it up further.
          while (section.Length > 1024)
          {
            // Find the last \n before the 1024-character limit. If no newline is found, cut off at 1024 characters.
            int splitIndex = section.LastIndexOf('\n', 1024);
            if (splitIndex <= 0) splitIndex = 1024;

            // Add the part before the split index to the result and remove it from the section.
            result.Add(section[..splitIndex].Trim() + (splitIndex == 1024 ? "..." : string.Empty));
            section = section[splitIndex..].Trim();
          }

          // Add any remaining part that is less than or equal to 1024 characters
          if (section.Length > 0)
            result.Add(section);

          return result;
        })
        .ToList();

    // If the rework has no description, put a hint for that.
    if (descriptionParts.Count == 0)
      descriptionParts.Add("*This rework has no description.*");

    EmbedBuilder embed = BaseEmbed
    .WithTitle($"{rework.Id} {rework.Name} ({rework.Code}) v{rework.PPVersion}")
    .WithUrl($"{rework.Url}")
    .AddField("Description", descriptionParts[0]);

    // Add the description parts to the embed.
    foreach (string part in descriptionParts.Skip(1))
      embed = embed.AddField("\u200B", part);

    string github = rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})";
    embed = embed
      .AddField("Ruleset", rework.RulesetName, true)
      .AddField("Links", $"[Huismetbenen]({rework.Url}) • {github}", true)
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
    // Construct the PP info string.
    string ppStr = $"▸ **PP**: {GetPPDifferenceText(local.OldPP, local.NewPP)}";
    ppStr += $"\n▸ **Aim**: {GetPPDifferenceText(live.AimPP, local.AimPP)}";
    ppStr += $"\n▸ **Tap**: {GetPPDifferenceText(live.TapPP, local.TapPP)}";
    ppStr += $"\n▸ **Acc**: {GetPPDifferenceText(live.AccPP, local.AccPP)}";
    if (local.FLPP + live.FLPP > 0)
      ppStr += $"\n▸ **FL**: {GetPPDifferenceText(live.FLPP, local.FLPP)}";
    if (local.ReadingPP + live.ReadingPP > 0)
      ppStr += $"\n▸ **Read**: {GetPPDifferenceText(live.ReadingPP, local.ReadingPP)}";

    // Constructs some more strings for the embed.
    string osuProfile = $"[osu! profile](https://osu.ppy.sh/u/{local.Id})";
    string huisProfile = $"[Huis Profile](https://pp.huismetbenen.nl/player/{local.Id}/{rework.Code})";
    string huisRework = $"[Rework]({rework.Url})";
    string github = rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})";

    return BaseEmbed
      .WithColor(new Color(0x58A1FF))
      .WithAuthor($"{local.Name} on {rework.Name}", $"https://a.ppy.sh/{local.Id}", $"https://pp.huismetbenen.nl/player/{local.Id}/{rework.Code}")
      .AddField("PP Comparison (Live → Local)", ppStr, true)
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
  public static Embed Info(bool osuV1Available, bool osuV2Available, bool huisAvailable)
  {
    // Build an uptime string (eg. "4 hours, 22 minutes, 1 second") from the time since startup.
    TimeSpan uptime = DateTime.UtcNow - Program.STARTUP_TIME;
    string uptimeStr = string.Join(", ", new (int Value, string Unit)[]
    {
      (uptime.Days / 7, "week"),
      (uptime.Days % 7, "day"),
      (uptime.Hours, "hour"),
      (uptime.Minutes, "minute"),
      (uptime.Seconds, "second")
    }.Where(x => x.Value > 0).Select(x => $"{x.Value} {x.Unit}{(x.Value > 1 ? "s" : "")}"));

    return BaseEmbed
      .WithColor(new Color(0xFFD4A8))
      .WithTitle($"Information about Huisbot {Program.VERSION}")
      .WithDescription("This bot aims to provide interaction with [Huismetbenen](https://pp.huismetbenen.nl/) via Discord and is dedicated to the " +
                       "[Official PP Discord](https://discord.gg/aqPCnXu). If any issues come up, please ping `@minisbett` or send them a DM.")
      .AddField("Uptime", $"{uptimeStr}\n\n[Source](https://github.com/minisbett/huisbot) • " +
                          $"[Add To Your Server](https://discord.com/oauth2/authorize?client_id=1174073630330716210&scope=bot&permissions=277025770560)", true)
      .AddField("API Status", $"osu!api v1 {new DEmoji(osuV1Available ? "✅" : "❌")}\nosu!api v2 {new DEmoji(osuV2Available ? "✅" : "❌")}\n" +
                              $"Huismetbenen {new DEmoji(huisAvailable ? "✅" : "❌")}", true)
      .WithThumbnailUrl("https://cdn.discordapp.com/attachments/1009893434087198720/1174333838579732581/favicon.png")
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the score calculation progress based on whether the local and live score have been calculated.
  /// </summary>
  /// <param name="local">The local rework to calculate.</param>
  /// <param name="reference">The reference rework to calculation.</param>
  /// <param name="localDone">Bool whether the local score finished calculation.</param>
  /// <returns>An embed for displaying the score calculation progress.</returns>
  public static Embed Calculating(HuisRework local, HuisRework? reference, bool localDone)
  {
    // Build the status string.
    string status = localDone ? "*Calculating reference score...*" : "*Calculating local score...*";
    status += $"\n\n{new DEmoji(localDone ? "✅" : "⏳")} {local.Name}";
    if (reference is not null)
      status += $"\n{new DEmoji(localDone ? "⏳" : "🕐")} {reference.Name}";

    return BaseEmbed
      .WithDescription(status)
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the difference between two calculated scores, optionally attributing it to a specific osu! user.
  /// </summary>
  /// <param name="local">The local calculated score for comparison.</param>
  /// <param name="reference">The calculated reference score for comparison.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="refRework">The reference rework.</param>
  /// <param name="beatmap">The beatmap.</param>
  /// <param name="user">The user the real score is based on.</param>
  /// <returns>An embed for displaying a the calculated score in comparison to the reference score.</returns>
  public static Embed CalculatedScore(HuisCalculationResponse local, HuisCalculationResponse reference, HuisRework rework, HuisRework refRework, OsuBeatmap beatmap, OsuScore? score = null, OsuUser? user = null)
  {
    // Construct the PP info field.
    string ppFieldTitle = rework == refRework ? "PP Overview" : "PP Comparison (Ref → Local)";
    string ppFieldText = $"▸ **PP**: {GetPPDifferenceText(reference.PerformanceAttributes.PP, local.PerformanceAttributes.PP)}";
    ppFieldText += $"\n▸ **Aim**: {GetPPDifferenceText(reference.PerformanceAttributes.AimPP, local.PerformanceAttributes.AimPP)}";
    ppFieldText += $"\n▸ **Tap**: {GetPPDifferenceText(reference.PerformanceAttributes.TapPP, local.PerformanceAttributes.TapPP)}";
    ppFieldText += $"\n▸ **Acc**: {GetPPDifferenceText(reference.PerformanceAttributes.AccPP, local.PerformanceAttributes.AccPP)}";
    if (local.PerformanceAttributes.FLPP + reference.PerformanceAttributes.FLPP > 0) // Check if both are 0 => FL PP probably doesn't exist
      ppFieldText += $"\n▸ **FL**: {GetPPDifferenceText(reference.PerformanceAttributes.FLPP, local.PerformanceAttributes.FLPP)}";
    if (local.PerformanceAttributes.ReadingPP is not null) // For reading PP, if not available it's null instead of 0 as it is with FL
      ppFieldText += $"\n▸ **Read**: {GetPPDifferenceText(reference.PerformanceAttributes.ReadingPP ?? 0, local.PerformanceAttributes.ReadingPP.Value)}";

    // Construct the score info field.
    string scoreFieldText = $"▸ {local.Score.Accuracy:N2}% ▸ {local.Score.MaxCombo}/{beatmap.MaxCombo}x";
    scoreFieldText += $"\n▸ {local.Score.Statistics.Count300} {_emojis["300"]} {local.Score.Statistics.Count100} {_emojis["100"]} {local.Score.Statistics.Count50} {_emojis["50"]} {local.Score.Statistics.Misses} {_emojis["miss"]}";
    scoreFieldText += "\n";
    if (!local.Score.Mods.IsClassic) // With classic mod, these statistics are irrelevant
      scoreFieldText += $"▸ {local.Score.Statistics.LargeTickMisses ?? 0} {_emojis["largetickmiss"]} {beatmap.SliderCount - local.Score.Statistics.SliderTailHits ?? beatmap.SliderCount} {_emojis["slidertailmiss"]} ";
    scoreFieldText += $"▸ {beatmap.CircleCount} {_emojis["circles"]} {beatmap.SliderCount} {_emojis["sliders"]} {beatmap.SpinnerCount} {_emojis["spinners"]}";
    scoreFieldText += $"\n▸ CS **{beatmap.GetAdjustedCS(local.Score.Mods):0.#}** AR **{beatmap.GetAdjustedAR(local.Score.Mods):0.#}** ▸ **{Math.Round(beatmap.GetBPM(local.Score.Mods))}** {_emojis["bpm"]}";
    scoreFieldText += $"\n▸ OD **{beatmap.GetAdjustedOD(local.Score.Mods):0.#}** HP **{beatmap.GetAdjustedHP(local.Score.Mods):0.#}** ▸ [visualizer](https://preview.tryz.id.vn/?b={beatmap.Id})";
    if (local.PerformanceAttributes.Deviation is not null)
      scoreFieldText += $"\n▸ **{local.PerformanceAttributes.Deviation:F2}** dev. / **{local.PerformanceAttributes.SpeedDeviation:F2}** speed dev.";

    // Add blank lines to fill up the pp comparison to match the line count of the score info and append the hyperlinks.
    ppFieldText += "".PadLeft(scoreFieldText.Split('\n').Length - ppFieldText.Split('\n').Length, '\n');
    ppFieldText += $"▸ [Huis Rework]({rework.Url}) • {(rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})")}";

    // Construct some more strings for the embed.
    (double refDiff, double localDiff) = (reference.DifficultyAttributes.DifficultyRating, local.DifficultyAttributes.DifficultyRating);
    string diffComparison = localDiff == refDiff ? localDiff.ToString("N2") : $"{refDiff:N2}→{localDiff:N2}";
    string title = $"{beatmap.Artist} - {beatmap.Title} [{beatmap.Version}]{local.Score.Mods.PlusString} ({diffComparison}★)";
    string reworkComparison = rework == refRework ? rework.Name! : $"{refRework.Name} → {rework.Name}";

    EmbedAuthorBuilder author = new();
    if(user is not null)
      author = new EmbedAuthorBuilder()
        .WithName($"{user.Name}: {user.PP:N}pp (#{user.GlobalRank:N0} | #{user.CountryRank:N0} {user.Country})")
        .WithIconUrl($"https://a.ppy.sh/{user.Id}")
        .WithUrl($"https://osu.ppy.sh/u/{user.Id}");

    return BaseEmbed
      .WithColor(new Color(0x4061E9))
      .WithTitle(title)
      .WithAuthor(author)
      .AddField(ppFieldTitle, ppFieldText, true)
      .AddField("Score Info", scoreFieldText, true)
      .WithUrl(score is null ? $"https://osu.ppy.sh/b/{beatmap.Id}" : $"https://osu.ppy.sh/s/{score.Id}")
      .WithImageUrl($"https://assets.ppy.sh/beatmaps/{beatmap.SetId}/covers/slimcover@2x.jpg")
      .WithFooter($"{reworkComparison} • {BaseEmbed.Footer.Text}", BaseEmbed.Footer.IconUrl)
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
  public static Embed BeatmapAliases(IEnumerable<BeatmapAlias> aliases)
  {
    // Sort the aliases by alphabetical order.
    aliases = aliases.OrderBy(x => x.Alias);

    // Build the alias string.
    string aliasesStr = "*There are no beatmap aliases. You can add some via `/alias beatmap add`.*";
    if (aliases.Any())
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
  public static Embed ScoreAliases(IEnumerable<ScoreAlias> aliases)
  {
    // Sort the aliases by alphabetical order.
    aliases = aliases.OrderBy(x => x.Alias);

    // Build the alias string.
    string aliasesStr = "*There are no score aliases. You can add some via `/alias score add`.*";
    if (aliases.Any())
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
    List<string> description =
    [
      $"*{rework.Name}*",
      $"[Huis Rework]({rework.Url}) • {(rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})")}",
      ""
    ];

    int offset = (page - 1) * SCORES_PER_PAGE;
    foreach (HuisScore score in allScores.Skip((page - 1) * SCORES_PER_PAGE).Take(SCORES_PER_PAGE))
    {
      // Add the info to the description lines.
      description.Add($"**#{++offset}** [{score.Username}](https://osu.ppy.sh/u/{score.UserId}) on " +
                      $"[{FormatScoreText(score)}](https://osu.ppy.sh/b/{score.BeatmapId})");
      description.Add($"▸ {GetPPDifferenceText(score.LivePP, score.LocalPP)} ▸ {score.Accuracy:N2}% {score.MaxCombo}x " +
                      $"▸ {score.Count100} {_emojis["100"]} {score.Count50} {_emojis["50"]} {score.Misses} {_emojis["miss"]}");
    }

    description.Add($"\n*Displaying scores {page * SCORES_PER_PAGE - (SCORES_PER_PAGE - 1)}-" +
                    $"{Math.Min(allScores.Length, page * SCORES_PER_PAGE)} of {allScores.Length} on page {page} of " +
                    $"{Math.Ceiling(allScores.Length * 1d / SCORES_PER_PAGE)}.*");

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
  /// <param name="sort">The sorting for the scores.</param>
  /// <param name="scoreType">The type of top scores being displayed.</param>
  /// <param name="page">The page being displayed.</param>
  /// <returns>An embed for displaying the top plays.</returns>
  public static Embed TopPlays(OsuUser user, HuisScore[] rawScores, HuisScore[] sortedScores, HuisRework rework, Sort sort, string scoreType, int page)
  {
    // Generate the embed description.
    List<string> description =
    [
      $"*{rework.Name}*",
      $"[osu! profile](https://osu.ppy.sh/u/{user.Id}) • [Huis Profile](https://pp.huismetbenen.nl/player/{user.Id}/{rework.Code})"
    + $" • [Huis Rework]({rework.Url}) • {(rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})")}",
      ""
    ];

    // Go through all scores and populate the description.
    foreach (HuisScore score in sortedScores.Skip((page - 1) * SCORES_PER_PAGE).Take(SCORES_PER_PAGE).ToArray())
    {
      // Get the placement of each score, as well as the difference.
      int placement = rawScores.ToList().IndexOf(score) + 1;
      int placementDiff = rawScores.OrderByDescending(x => x.LivePP).ToList().IndexOf(score) + 1 - placement;
      string placementStr = $"**#{placement}**" + (placementDiff != 0 ? $" ({placementDiff:+#;-#;0})" : "");

      // Add the info to the description lines.
      description.Add($"{placementStr} [{FormatScoreText(score)}](https://osu.ppy.sh/b/{score.BeatmapId})");
      description.Add($"▸ {GetPPDifferenceText(score.LivePP, score.LocalPP)} ▸ {score.Accuracy:N2}% {score.MaxCombo}x " +
                      $"▸ {score.Count100} {_emojis["100"]} {score.Count50} {_emojis["50"]} {score.Misses} {_emojis["miss"]}");
    }

    description.Add($"\n*Displaying scores {page * SCORES_PER_PAGE - (SCORES_PER_PAGE - 1)}-" +
                    $"{Math.Min(rawScores.Length, page * SCORES_PER_PAGE)} of {rawScores.Length} on page {page} of " +
                    $"{Math.Ceiling(rawScores.Length * 1d / SCORES_PER_PAGE)}.*");

    string scoreTypeStr = scoreType switch
    {
      "topranks" => "Top Scores",
      "flashlight" => "Flashlight Scores",
      "pinned" => "Pinned Scores",
      _ => "<unknown> Scores"
    };

    return BaseEmbed
      .WithTitle($"{scoreTypeStr} of {user.Name} ({sort.DisplayName})")
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
    HuisPlayer[] players = allPlayers.Skip((page - 1) * PLAYERS_PER_PAGE).Take(PLAYERS_PER_PAGE).ToArray();

    // Generate the embed description.
    string github = rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})";
    List<string> description =
    [
      $"*{rework.Name}*",
      $"[Huis Rework]({rework.Url}) • {github}",
      ""
    ];
    List<string> playerStrs = [];
    List<string> ppOldStrs = [];
    List<string> ppNewStrs = [];
    foreach (HuisPlayer player in players)
    {
      double oldPP = Math.Round(player.OldPP);
      double newPP = Math.Round(player.NewPP);
      playerStrs.Add($"**#{player.Rank?.ToString() ?? "-"}** [{player.Name}](https://osu.ppy.sh/u/{player.Id})");
      ppOldStrs.Add(oldPP.ToString("#,##0.##"));
      ppNewStrs.Add($"**{newPP:#,##0.##}pp**" + (oldPP != newPP ? $" ({newPP - oldPP:+#,##0.##;-#,##0.##}pp)" : ""));
    }

    // Generate the page information footer.
    string pageInfo = $"\n*Displaying players {page * PLAYERS_PER_PAGE - (PLAYERS_PER_PAGE - 1)}-" +
                      $"{Math.Min(players.Length, page * PLAYERS_PER_PAGE)} on page {page} of " +
                      $"{Math.Ceiling(allPlayers.Length * 1d / PLAYERS_PER_PAGE)}.*";

    return BaseEmbed
      .WithTitle($"Player Rankings ({sort.DisplayName})")
      .WithDescription(string.Join("\n", description))
      .AddField("Player", string.Join("\n", playerStrs), true)
      .AddField("Live PP", string.Join("\n", ppOldStrs), true)
      .AddField("Local PP", string.Join("\n", ppNewStrs), true)
      .AddField("\u200B", pageInfo)
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
  /// Returns an embed for displaying the difficulty attributes of a score.
  /// </summary>
  /// <param name="score">The calculated score.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="beatmap">The beatmap.</param>
  /// <returns>An embed for displaying the difficulty attributes.</returns>
  public static Embed DifficultyAttributes(HuisCalculationResponse score, HuisRework rework, OsuBeatmap beatmap)
  {
    // Construct some strings for the embed.
    string difficulty = $"Aim: **{score.DifficultyAttributes.AimDifficulty:N2}★**\nSpeed: **{score.DifficultyAttributes.SpeedDifficulty:N2}★**";
    difficulty += score.DifficultyAttributes.FlashlightDifficulty is null ? "" : $"\nFL: **{score.DifficultyAttributes.FlashlightDifficulty:N2}★**";
    string strainCounts = $"Aim: **{score.DifficultyAttributes.AimDifficultStrainCount:N2}**\n"
                        + $"Speed: **{score.DifficultyAttributes.SpeedDifficultStrainCount:N2}**";
    string visualizer = $"[map visualizer](https://preview.tryz.id.vn/?b={beatmap.Id})";
    string osu = $"[osu! page](https://osu.ppy.sh/b/{beatmap.Id})";
    string huisRework = $"[Huis Rework]({rework.Url})";
    string github = rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})";

    return BaseEmbed
    .WithColor(new Color(0x4061E9))
      .WithTitle($"{beatmap.Artist} - {beatmap.Title} [{beatmap.Version}]{score.Score.Mods.PlusString} ({score.DifficultyAttributes.DifficultyRating:N2}★)")
      .AddField("Difficulty", difficulty, true)
      .AddField("Difficult Strains", strainCounts, true)
      .AddField("Slider Factor", $"{score.DifficultyAttributes.SliderFactor:N5}", true)
      .AddField($"Speed Notes: {score.DifficultyAttributes.SpeedNoteCount:N2}", visualizer, true)
      .AddField($"Max Combo: {beatmap.MaxCombo}x", osu, true)
      .AddField($"OD {beatmap.GetAdjustedOD(score.Score.Mods):0.##} AR {beatmap.GetAdjustedAR(score.Score.Mods):0.##}", $"{huisRework} • {github}", true)
      .WithUrl($"https://osu.ppy.sh/b/{beatmap.Id}")
      .WithImageUrl($"https://assets.ppy.sh/beatmaps/{beatmap.SetId}/covers/slimcover@2x.jpg")
      .WithFooter($"{rework.Name} • {BaseEmbed.Footer.Text}", BaseEmbed.Footer.IconUrl)
    .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the feedback of a user.
  /// </summary>
  /// <param name="user">The Discord user submitting the feedback.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="text">The text body of the feedback.</param>
  /// <returns>An embed for displaying the feedback of a user.</returns>
  public static Embed Feedback(IUser user, HuisRework rework, string text) => BaseEmbed
    .WithColor(new Color(0x4287f5))
    .WithAuthor($"{user.Username} ({user.Id})", user.GetAvatarUrl())
    .WithTitle($"{rework.Name!} (v{rework.PPVersion})")
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
    // Round the PP values, as decimals are irrelevant info and hurts the display flexibility.
    oldPP = Math.Round(oldPP);
    newPP = Math.Round(newPP);

    // If the PP do not differ, simply return the PP value.
    if (newPP == oldPP)
      return $"**{oldPP:0.##}pp**";

    // Otherwise return the difference string.
    return $"{oldPP:0.##}pp → **{newPP:0.##}pp** ({newPP - oldPP:+#,##0.##;-#,##0.##}pp)";
  }

  /// <summary>
  /// Shortens the display text (eg. "Save Me [Nightmare] +HDDTCL") of a score to fit better into an embed.
  /// - If the title + version > 60 and version > 27, it trims the version to 27 characters
  /// - If title + version still > 60, it trims the title to 27 characters
  /// </summary>
  /// <returns>The shortened display text.</returns>
  private static string FormatScoreText(HuisScore score)
  {
    // Trim the version if title + version is too long. If it's still too long, trim title as well.
    string title = score.Title ?? "";
    string version = score.Version ?? "";
    if ($"{title} [{version}]".Length > 60 && version.Length > 27)
      version = $"{version[..27]}...";
    if ($"{title} [{version}]".Length > 60)
      title = $"{title[..27]}...";

    return $"{title} [{version}] {score.Mods}".TrimEnd(' ');
  }

  /// <summary>
  /// A dictionary with identifiers for emojis and their corresponding <see cref="Emoji"/> object.
  /// </summary>
  private static readonly Dictionary<string, Emoji> _emojis = new()
  {
    { "XH", new("rankSSH", 1159888184600170627) },
    { "X", new("rankSS", 1159888182075207740) },
    { "SH", new("rankSH", 1159888343245537300) },
    { "S", new("rankS", 1159888340536012921) },
    { "A", new("rankA", 1159888148080361592) },
    { "B", new("rankB", 1159888151771369562) },
    { "C", new("rankC", 1159888154891919502) },
    { "D", new("rankD", 1159888158150893678) },
    { "F", new("rankF", 1159888321342865538) },
    { "300", new("300", 1159888146448797786) },
    { "100", new("100", 1159888144406171719) },
    { "50", new("50", 1159888143282094221) },
    { "miss", new("miss", 1159888326698995842)},
    { "largetickmiss", new("largetickmiss", 1340259318489944075) },
    { "slidertailmiss", new("slidertailmiss", 1340117215210635274) },
    { "loved", new("loved", 1159888325491036311) },
    { "qualified", new("approved", 1159888150542418031) },
    { "approved", new("approved", 1159888150542418031) },
    { "ranked", new("ranked", 1159888338199773339) },
    { "length", new("length", 1159888322873786399) },
    { "bpm", new("length", 1159888153000280074) },
    { "circles", new("circles", 1159888155902758953) },
    { "sliders", new("sliders", 1159888389902970890) },
    { "spinners", new("spinners", 1159888345250414723) },
    { "osu", new("std", 1159888333044981913) },
    { "taiko", new("taiko", 1159888334492029038) },
    { "fruits", new("fruits", 1159888328984903700) },
    { "mania", new("mania", 1159888330637463623) },
  };
}

/// <summary>
/// Represents a Discord emoji with a name and ID.
/// </summary>
/// <remarks>
/// Creates a new <see cref="Emoji"/> object with the name and ID of the custom emoji.
/// </remarks>
/// <param name="name">The name of the emoji.</param>
/// <param name="id">The ID of the emoji.</param>
public class Emoji(string name, ulong id)
{
  /// <summary>
  /// The name of the emoji.
  /// </summary>
  public string Name { get; } = name;

  /// <summary>
  /// The snowflake ID of the emoji.
  /// </summary>
  public ulong Id { get; } = id;

  /// <summary>
  /// Returns the asset url of this emoji.
  /// </summary>
  public string Url => $"https://cdn.discordapp.com/emojis/{Id}.webp";

  /// <summary>
  /// Returns the emoji string representation of this emoji.
  /// </summary>
  public override string ToString() => $"<:{Name}:{Id}>";
}
