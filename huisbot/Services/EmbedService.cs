using Discord;
using huisbot.Helpers;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;

namespace huisbot.Services;

/// <summary>
/// Provides embeds for the application.
/// </summary>
public class EmbedService(DiscordService discord)
{
  /// <summary>
  /// A dictionary of all application-specific emotes, keyed by their name.
  /// </summary>
  private Dictionary<string, Emote> Emojis => discord.ApplicationEmotes.ToDictionary(x => x.Name, x => x);

  /// <summary>
  /// The amount of scores to display per page.<br/>
  /// <see cref="TopPlays(OsuUser, HuisScore[], HuisScore[], HuisRework, Sort, string, int)"/><br/>
  /// <see cref="ScoreRankings(HuisScore[], HuisRework, Sort, int)"/>
  /// </summary>
  public const int SCORES_PER_PAGE = 10;

  /// <summary>
  /// The amount of top players to display per page.<br/>
  /// <see cref="PlayerRankings(HuisPlayer[], HuisRework, Sort, int)"/>
  /// </summary>
  public const int PLAYERS_PER_PAGE = 20;

  /// <summary>
  /// Returns a base embed all other embeds are based on.
  /// </summary>
  private EmbedBuilder BaseEmbed => new EmbedBuilder()
    .WithFooter($"huisbot v{Program.VERSION} by minisbett", "https://pp.huismetbenen.nl/favicon.ico")
    .WithCurrentTimestamp();

  /// <summary>
  /// Returns an error embed for displaying an internal error message.
  /// </summary>
  /// <param name="message">The error message.</param>
  /// <returns>An embed for displaying an internal error message.</returns>
  public Embed InternalError(string message) => BaseEmbed
    .WithColor(Color.Red)
    .WithTitle("An internal error occured.")
    .WithDescription(message)
    .Build();

  /// <summary>
  /// Returns an error embed for displaying an error message.
  /// </summary>
  /// <param name="message">The error message.</param>
  /// <returns>An embed for displaying an error message.</returns>
  public Embed Error(string message) => BaseEmbed
    .WithColor(Color.Red)
    .WithDescription(message)
    .Build();

  /// <summary>
  /// Returns an error embed for displaying a neutral message.
  /// </summary>
  /// <param name="message">The neutral message.</param>
  /// <returns>An embed for displaying a neutral message.</returns>
  public Embed Neutral(string message) => BaseEmbed
    .WithDescription(message)
    .Build();

  /// <summary>
  /// Returns an error embed for displaying a success message.
  /// </summary>
  /// <param name="message">The success message.</param>
  /// <returns>An embed for displaying a success message.</returns>
  public Embed Success(string message) => BaseEmbed
    .WithColor(Color.Green)
    .WithDescription(message)
    .Build();

  /// <summary>
  /// Returns an embed notifying the user that they lack Onion permissions.
  /// </summary>
  public Embed NotOnion => BaseEmbed
    .WithColor(Color.Red)
    .WithTitle("Insufficient permissions.")
    .WithDescription("You need **Onion** permissions in order to access this rework.\nYou can apply for the Onion role here:\n[PP Discord](https://discord.gg/aqPCnXu) • <#1020389783110955008>")
    .Build();

  /// <summary>
  /// Returns an embed notifying the user that they lack PP Team permissions.
  /// </summary>
  public Embed NotPPTeam => BaseEmbed
    .WithColor(Color.Red)
    .WithTitle("Insufficient permissions.")
    .WithDescription("You need **PP Team** permissions in order to use this command.")
    .Build();

  #region Huis

  /// <summary>
  /// Returns an embed with the specified rework.
  /// </summary>
  /// <param name="rework">The rework to display.</param>
  public Embed Rework(HuisRework rework)
  {
    // Parse the rich text description of the rework into markdown, taking unsupported headers into consideration.
    string description = rework.Description!;
    foreach (string tag in Enumerable.Range(1, 4).SelectMany(x => new string[] { $"<h{x}>", $"</h{x}>" }))
      description = description.Replace(tag, "");
    description = new ReverseMarkdown.Converter().Convert(description);

    // Divide the description in multiple parts due to the 1024 character limit.
    // Step 1: Split the description into paragraphs (\n\n)
    // Step 2: Inside those paragraphs, if length > 1024 cut at the last \n
    //         that'd still ensure <= 1024 length until the paragraph is processed
    // Step 3: If the parts are still too big, cut them off with "..."
    List<string> descriptionParts = description
        .Split("\n\n")
        .SelectMany(section =>
        {
          List<string> result = [];

          // If the paragraph is more than 1024 characters, split it up further.
          while (section.Length > 1024)
          {
            // Find the last \n before the 1024-character limit. If no newline is found, cut off at 1024 characters.
            int splitIndex = section.LastIndexOf('\n', 1024);
            if (splitIndex <= 0)
              splitIndex = 1024;

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

    if (descriptionParts.Count == 0)
      descriptionParts.Add("*This rework has no description.*");

    EmbedBuilder embed = BaseEmbed
      .WithColor(new Color(0xFFD4A8))
      .WithTitle($"{rework.Id}: {rework.Name} ({rework.Code})")
      .WithUrl($"{rework.Url}")
      .AddField("Description", descriptionParts[0]);

    // Add the description parts to the embed in separate fields with an invisible title.
    foreach (string part in descriptionParts.Skip(1))
      embed = embed.AddField("\u200B", part);

    embed = embed
      .AddField("Info", $"{rework.RulesetName} • Version {rework.PPVersion}", true)
      .AddField("Links", $"[Huismetbenen]({rework.Url}) • {(rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})")}", true)
      .AddField("Status", rework.ReworkTypeString, true);

    return embed.Build();
  }

  /// <summary>
  /// Returns an embed for displaying the specified player in the specified rework.
  /// </summary>
  /// <param name="local">The player to display.</param>
  /// <param name="rework">The rework.</param>
  public Embed Player(HuisPlayer local, HuisPlayer live, HuisRework rework)
  {
    string ppStr = $"""
                    ▸ **PP**: {GetPPDifferenceText(local.OldPP, local.NewPP)}
                    ▸ **Aim**: {GetPPDifferenceText(live.AimPP, local.AimPP)}
                    ▸ **Tap**: {GetPPDifferenceText(live.TapPP, local.TapPP)}
                    ▸ **Acc**: {GetPPDifferenceText(live.AccPP, local.AccPP)}
                    {(local.FLPP + live.FLPP > 0 ? $"▸ **FL**: {GetPPDifferenceText(live.FLPP, local.FLPP)}" : "")}
                    """;

    string links = $"""
                    ▸ [osu! profile](https://osu.ppy.sh/u/{local.Id})
                    ▸ [Huis Profile](https://pp.huismetbenen.nl/player/{local.Id}/{rework.Code})
                    ▸ [Rework]({rework.Url})
                    ▸ {(rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})")}
                    """;

    return BaseEmbed
      .WithColor(new Color(0xFFD4A8))
      .WithAuthor($"{local.Name} on {rework.Name}", $"https://a.ppy.sh/{local.Id}", $"https://pp.huismetbenen.nl/player/{local.Id}/{rework.Code}")
      .AddField("PP Comparison (Live → Local)", ppStr, true)
      .AddField("Useful Links", links, true)
      .WithFooter($"{BaseEmbed.Footer.Text} • Last Updated", BaseEmbed.Footer.IconUrl)
      .WithTimestamp(local.LastUpdated)
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying info about the bot (version, uptime, api status, ...).
  /// </summary>
  /// <param name="osuApiV1">Bool whether the osu! API v1 is available.</param>
  /// <param name="osuApiV2">Bool whether the osu! API v2 is available.</param>
  /// <param name="huisApi">Bool whether the Huis api is available.</param>
  /// <param name="guildInstalls">The amount of guilds the application is installed in.</param>
  /// <param name="userInstalls">The amount of users that installed the application.</param>
  public Embed Info(bool osuApiV1, bool osuApiV2, bool huisApi, int guildInstalls, int userInstalls)
  {
    // Build an uptime string (eg. "4 hours, 22 minutes, 1 second") from the time since startup.
    string uptimeStr = string.Join(", ", new (int Value, string Unit)[]
    {
      (discord.Uptime.Days / 7, "week"),
      (discord.Uptime.Days % 7, "day"),
      (discord.Uptime.Hours, "hour"),
      (discord.Uptime.Minutes, "minute"),
      (discord.Uptime.Seconds, "second")
    }.Where(x => x.Value > 0).Select(x => $"{x.Value} {x.Unit}{(x.Value > 1 ? "s" : "")}"));

    string installations = $"""
                            {guildInstalls} Guilds
                            {userInstalls} Users
                            [Add To Your Server](https://discord.com/oauth2/authorize?client_id=1174073630330716210&scope=bot&permissions=277025770560)
                            """;

    string status = $"""
                     {(osuApiV1 ? "✅" : "❌")} osu!api v1
                     {(osuApiV2 ? "✅" : "❌")} osu!api v2
                     {(huisApi ? "✅" : "❌")} Huismetbenen
                     """;

    return BaseEmbed
      .WithColor(new Color(0xFFD4A8))
      .WithTitle($"Information about Huisbot {Program.VERSION}")
      .WithDescription("This bot aims to provide interaction with [Huismetbenen](https://pp.huismetbenen.nl/) via Discord and is dedicated to the [Official PP Discord](https://discord.gg/aqPCnXu). If any issues come up, please ping `@minisbett` or send them a DM.")
      .AddField("Uptime", $"{uptimeStr}\n{(uptimeStr.Count(x => x == ',') <= 1 ? "\n" : "")}[Source Code](https://github.com/minisbett/huisbot)", true)
      .AddField("Installation Count", installations, true)
      .AddField("API Status", status, true)
      .WithThumbnailUrl("https://pp.huismetbenen.nl/favicon.png")
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the score calculation progress.
  /// </summary>
  /// <param name="local">The local rework to calculate.</param>
  /// <param name="reference">The reference rework to calculate. If null, the embed only displays the calculation of the local rework.</param>
  /// <param name="localDone">Bool whether the local score finished calculation, showing the specified reference score as currently calculating.</param>
  public Embed Calculating(HuisRework local, HuisRework? reference, bool localDone) => BaseEmbed
    .WithColor(new Color(0xFFD4A8))
    .WithDescription($"""
                      {(localDone ? "*Calculating reference score...*" : "*Calculating local score...*")}

                      {(localDone ? "✅" : "⏳")} {local.Name}
                      {(reference is null ? "" : $"{(localDone ? "⏳" : "🕐")} {reference.Name}")}
                      """)
    .Build();

  /// <summary>
  /// Returns an embed for displaying the difference between two calculated scores, optionally attributing it to a specific osu! user.
  /// </summary>
  /// <param name="local">The local calculated score for comparison.</param>
  /// <param name="reference">The calculated reference score for comparison.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="refRework">The reference rework.</param>
  /// <param name="beatmap">The beatmap.</param>
  /// <param name="user">The user the real score is based on.</param>
  public Embed CalculatedScore(HuisCalculationResponse local, HuisCalculationResponse reference, HuisRework rework, HuisRework refRework, OsuBeatmap beatmap, OsuScore? score = null, OsuUser? user = null)
  {
    string ppText = $"""
                     ▸ **PP**: {GetPPDifferenceText(reference.PerformanceAttributes.PP, local.PerformanceAttributes.PP)}
                     ▸ **Aim**: {GetPPDifferenceText(reference.PerformanceAttributes.AimPP, local.PerformanceAttributes.AimPP)}
                     ▸ **Tap**: {GetPPDifferenceText(reference.PerformanceAttributes.TapPP, local.PerformanceAttributes.TapPP)}
                     ▸ **Acc**: {GetPPDifferenceText(reference.PerformanceAttributes.AccPP, local.PerformanceAttributes.AccPP)}
                     {(local.Score.Mods.IsFlashlight ? $"▸ **FL**: {GetPPDifferenceText(reference.PerformanceAttributes.FLPP, local.PerformanceAttributes.FLPP)}" : "")}
                     ▸ [Huis Rework]({rework.Url}) • {(rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})")}
                     """.Replace("\r\n\r\n", "\r\n"); // Remove the blank line if this is not a flashlight score

    #region score components
    string acc = $"{local.Score.Accuracy:N2}%";
    string combo = $"{local.Score.MaxCombo}/{beatmap.MaxCombo}x";
    string hit300 = $"{local.Score.Statistics.Count300} {Emojis["300"]}";
    string hit100 = $"{local.Score.Statistics.Count100} {Emojis["100"]}";
    string hit50 = $"{local.Score.Statistics.Count50} {Emojis["50"]}";
    string misses = $"{local.Score.Statistics.Misses} {Emojis["miss"]}";
    string ltm = $"{local.Score.Statistics.LargeTickMisses ?? 0} {Emojis["largetickmiss"]}";
    string stm = $"{beatmap.SliderCount - local.Score.Statistics.SliderTailHits ?? beatmap.SliderCount} {Emojis["slidertailmiss"]}";
    string circles = $"{beatmap.CircleCount} {Emojis["circles"]}";
    string sliders = $"{beatmap.SliderCount} {Emojis["sliders"]}";
    string spinners = $"{beatmap.SpinnerCount} {Emojis["spinners"]}";
    string CSAR = $"`CS {beatmap.GetAdjustedCS(local.Score.Mods):N1} AR {beatmap.GetAdjustedAR(local.Score.Mods):N1}`";
    string ODHP = $"`OD {beatmap.GetAdjustedOD(local.Score.Mods):N1} HP {beatmap.GetAdjustedHP(local.Score.Mods):N1}`";
    string bpm = $"**{Math.Round(beatmap.GetBPM(local.Score.Mods))}** {Emojis["bpm"]}";
    string visualizer = $"[visualizer](https://preview.tryz.id.vn/?b={beatmap.Id})";
    #endregion
    string scoreText = $"""
                        ▸ {acc} ▸ {combo}
                        ▸ {hit300} {hit100} {hit50} {misses}
                        ▸ {(local.Score.Mods.IsClassic ? "" : $"{ltm} {stm}")} {circles} {sliders} {spinners}
                        ▸ {CSAR} ▸ {bpm}
                        ▸ {ODHP} ▸ {visualizer}
                        """.Replace("▸  ", "▸ "); // Remove the double whitespace if this is a classic score (no ltm and stm)

    // Construct the difficulty rating comparison string (eg. "10.65→10.66★" or "7.33★" if there is no change)
    (double refDiff, double localDiff) = (reference.DifficultyAttributes.DifficultyRating, local.DifficultyAttributes.DifficultyRating);
    string diffComparison = localDiff == refDiff ? localDiff.ToString("N2") : $"{refDiff:N2}→{localDiff:N2}";

    EmbedAuthorBuilder author = user is null ? new() : new EmbedAuthorBuilder()
        .WithName($"{user.Name}: {user.PP:N}pp (#{user.GlobalRank:N0} | #{user.CountryRank:N0} {user.Country})")
        .WithIconUrl($"https://a.ppy.sh/{user.Id}")
        .WithUrl($"https://osu.ppy.sh/u/{user.Id}");

    return BaseEmbed
      .WithColor(new Color(0xFFD4A8))
      .WithTitle($"{beatmap.Artist} - {beatmap.Title} [{beatmap.Version}]{local.Score.Mods.PlusString} ({diffComparison}★)")
      .WithAuthor(author)
      .AddField(rework == refRework ? "PP Overview" : "PP Comparison (Ref → Local)", ppText, true)
      .AddField("Score Info", scoreText, true)
      .WithUrl(score is null ? $"https://osu.ppy.sh/b/{beatmap.Id}" : $"https://osu.ppy.sh/scores/{score.Id}")
      .WithImageUrl($"https://assets.ppy.sh/beatmaps/{beatmap.SetId}/covers/slimcover@2x.jpg")
      .WithFooter($"{(rework == refRework ? rework.Name : $"{refRework.Name} → {rework.Name}")} • {BaseEmbed.Footer.Text}", BaseEmbed.Footer.IconUrl)
    .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the difficulty attributes of a score.
  /// </summary>
  /// <param name="score">The calculated score.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="beatmap">The beatmap.</param>
  public Embed DifficultyAttributes(HuisCalculationResponse score, HuisRework rework, OsuBeatmap beatmap)
  {
    // Construct some strings for the embed.
    string difficulty = $"""
                         Aim: **{score.DifficultyAttributes.AimDifficulty:N2} ★**
                         Speed: **{score.DifficultyAttributes.SpeedDifficulty:N2} ★**
                         {(score.DifficultyAttributes.FlashlightDifficulty is null ? "" : $"FL: **{score.DifficultyAttributes.FlashlightDifficulty:N2} ★**")}
                         """;
    string strainCounts = $"""
                           Aim: **{score.DifficultyAttributes.AimDifficultStrainCount:N2}**
                           Speed: **{score.DifficultyAttributes.SpeedDifficultStrainCount:N2}**
                           """;
    string mapAttributes = $"""
                            Max Combo: **{beatmap.MaxCombo}x**
                            Overall Difficulty: **{beatmap.GetAdjustedOD(score.Score.Mods):0.##}**
                            Approach Rate: **{beatmap.GetAdjustedAR(score.Score.Mods):0.##}**
                            """;
    string other = $"""
                    Speed Notes: {score.DifficultyAttributes.SpeedNoteCount:N2}
                    Slider Factor: {score.DifficultyAttributes.SliderFactor:N5} ({100 - score.DifficultyAttributes.SliderFactor * 100:N2}%)
                    [map visualizer](https://preview.tryz.id.vn/?b={beatmap.Id}) • [Huis Rework]({rework.Url}) • {(rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})")}
                    """;

    return BaseEmbed
      .WithColor(new Color(0xFFD4A8))
      .WithTitle($"{beatmap.Artist} - {beatmap.Title} [{beatmap.Version}]{score.Score.Mods.PlusString} ({score.DifficultyAttributes.DifficultyRating:N2}★)")
      .AddField("Difficulty", difficulty, true)
      .AddField("Difficult Strains", strainCounts, true)
      .AddField("\u200B", "\u200B", true)
      .AddField("Map Attributes", mapAttributes, true)
      .AddField("Other", other, true)
      .AddField("\u200B", "\u200B", true)
      .WithUrl($"https://osu.ppy.sh/b/{beatmap.Id}")
      .WithImageUrl($"https://assets.ppy.sh/beatmaps/{beatmap.SetId}/covers/slimcover@2x.jpg")
      .WithFooter($"{rework.Name} • {BaseEmbed.Footer.Text}", BaseEmbed.Footer.IconUrl)
    .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the top plays of the specified player in the specified rework.
  /// </summary>
  /// <param name="user">The player.</param>
  /// <param name="rawScores">All top plays in their original order. Used to calculate placement differences.</param>
  /// <param name="sortedScores">All top plays in their sorted order.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="sort">The sorting for the scores.</param>
  /// <param name="scoreType">The type of top scores being displayed.</param>
  /// <param name="page">The page being displayed.</param>
  public Embed TopPlays(OsuUser user, HuisScore[] rawScores, HuisScore[] sortedScores, HuisRework rework, Sort sort, string scoreType, int page)
  {
    string osuProfile = $"[osu! profile](https://osu.ppy.sh/u/{user.Id})";
    string huisProfile = $"[Huis Profile](https://pp.huismetbenen.nl/player/{user.Id}/{rework.Code})";
    string huisRework = $"[Huis Rework]({rework.Url})";
    string source = rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})";
    List<string> description =
    [
      $"*{rework.Name}*",
      $"{osuProfile} • {huisProfile} • {huisRework} • {source}",
      ""
    ];

    foreach (HuisScore score in sortedScores.Skip((page - 1) * SCORES_PER_PAGE).Take(SCORES_PER_PAGE).ToArray())
    {
      // Get the placement of each score, as well as the difference.
      int placement = rawScores.ToList().IndexOf(score) + 1;
      int placementDiff = rawScores.OrderByDescending(x => x.LivePP).ToList().IndexOf(score) + 1 - placement;
      string placementStr = $"**#{placement}**" + (placementDiff != 0 ? $" ({placementDiff:+#;-#;0})" : "");

      description.Add($"{placementStr} [{FormatScoreText(score)}](https://osu.ppy.sh/b/{score.BeatmapId})");
      description.Add($"▸ {GetPPDifferenceText(score.LivePP, score.LocalPP)} ▸ {score.Accuracy:N2}% {score.MaxCombo}x " +
                      $"▸ {score.Count100} {Emojis["100"]} {score.Count50} {Emojis["50"]} {score.Misses} {Emojis["miss"]}");
    }

    description.Add($"\n*Displaying scores {page * SCORES_PER_PAGE - (SCORES_PER_PAGE - 1)}-" +
                    $"{Math.Min(rawScores.Length, page * SCORES_PER_PAGE)} of {rawScores.Length} on page {page} of " +
                    $"{Math.Ceiling(rawScores.Length * 1d / SCORES_PER_PAGE)}.*");

    string scoreTypeStr = scoreType switch
    {
      "topranks" => "Top",
      "flashlight" => "Flashlight",
      "pinned" => "Pinned",
      _ => "???"
    };

    return BaseEmbed
      .WithColor(new Color(0xFFD4A8))
      .WithTitle($"{scoreTypeStr} Scores of {user.Name} ({sort.DisplayName})")
      .WithDescription(string.Join("\n", description))
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the player rankings in the specified rework with the specified players.
  /// </summary>
  /// <param name="players">All players, including those from any other page.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="sort">The sort option.</param>
  /// <param name="page">The page being displayed.</param>
  public Embed PlayerRankings(HuisPlayer[] players, HuisRework rework, Sort sort, int page)
  {
    // Generate the values for each of the three columns (player, old pp, new pp).
    List<string> playerStrs = [];
    List<string> ppOldStrs = [];
    List<string> ppNewStrs = [];
    foreach (HuisPlayer player in players.Skip((page - 1) * PLAYERS_PER_PAGE).Take(PLAYERS_PER_PAGE))
    {
      double oldPP = Math.Round(player.OldPP);
      double newPP = Math.Round(player.NewPP);
      playerStrs.Add($"**#{player.Rank?.ToString() ?? "-"}** [{player.Name}](https://osu.ppy.sh/u/{player.Id})");
      ppOldStrs.Add(oldPP.ToString("#,##0.##"));
      ppNewStrs.Add($"**{newPP:#,##0.##}pp**" + (oldPP != newPP ? $" ({newPP - oldPP:+#,##0.##;-#,##0.##}pp)" : ""));
    }

    string pageInfo = $"*Displaying players {page * PLAYERS_PER_PAGE - (PLAYERS_PER_PAGE - 1)}-" +
                      $"{Math.Min(players.Length, page * PLAYERS_PER_PAGE)} of {players.Length} on page {page} of " +
                      $"{Math.Ceiling(players.Length * 1d / PLAYERS_PER_PAGE)}.*";

    return BaseEmbed
      .WithColor(new Color(0xFFD4A8))
      .WithTitle($"Player Rankings ({sort.DisplayName})")
      .WithDescription($"*{rework.Name}*\n[Huis Rework]({rework.Url}) • {(rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})")}")
      .AddField("Player", string.Join("\n", playerStrs), true)
      .AddField("Live PP", string.Join("\n", ppOldStrs), true)
      .AddField("Local PP", string.Join("\n", ppNewStrs), true)
      .AddField("\u200B", pageInfo)
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the score rankings in the specified rework with the specified scores.
  /// </summary>
  /// <param name="scores">All scores, including those from any other page.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="sort">The sort option.</param>
  /// <param name="page">The page being displayed.</param>
  public Embed ScoreRankings(HuisScore[] scores, HuisRework rework, Sort sort, int page)
  {
    List<string> description =
    [
      $"*{rework.Name}*",
      $"[Huis Rework]({rework.Url}) • {(rework.CommitUrl is null ? "Source unavailable" : $"[Source]({rework.CommitUrl})")}",
      ""
    ];

    int scorePlacement = (page - 1) * SCORES_PER_PAGE; // Keep track of the placement of the score
    foreach (HuisScore score in scores.Skip((page - 1) * SCORES_PER_PAGE).Take(SCORES_PER_PAGE))
    {
      description.Add($"**#{++scorePlacement}** [{score.Username}](https://osu.ppy.sh/u/{score.UserId}) on " +
                      $"[{FormatScoreText(score)}](https://osu.ppy.sh/b/{score.BeatmapId})");
      description.Add($"▸ {GetPPDifferenceText(score.LivePP, score.LocalPP)} ▸ {score.Accuracy:N2}% {score.MaxCombo}x " +
                      $"▸ {score.Count100} {Emojis["100"]} {score.Count50} {Emojis["50"]} {score.Misses} {Emojis["miss"]}");
    }

    description.Add($"\n*Displaying scores {page * SCORES_PER_PAGE - (SCORES_PER_PAGE - 1)}-" +
                    $"{Math.Min(scores.Length, page * SCORES_PER_PAGE)} of {scores.Length} on page {page} of " +
                    $"{Math.Ceiling(scores.Length * 1d / SCORES_PER_PAGE)}.*");

    return BaseEmbed
      .WithColor(new Color(0xFFD4A8))
      .WithTitle($"Score Rankings ({sort.DisplayName})")
      .WithDescription(string.Join("\n", description))
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the feedback of a user.
  /// </summary>
  /// <param name="user">The Discord user submitting the feedback.</param>
  /// <param name="rework">The rework.</param>
  /// <param name="text">The text body of the feedback.</param>
  public Embed Feedback(IUser user, HuisRework rework, string text) => BaseEmbed
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
  private string GetPPDifferenceText(double oldPP, double newPP)
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
  private string FormatScoreText(HuisScore score)
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

  #endregion

  #region Other

  /// <summary>
  /// Returns an embed for displaying a successful link between a Discord and an osu! account.
  /// </summary>
  /// <param name="user">The osu! user that was linked.</param>
  public Embed LinkSuccessful(OsuUser user) => BaseEmbed
    .WithColor(Color.Green)
    .WithDescription($"Your Discord account was successfully linked to the osu! account `{user.Name}`.")
    .WithThumbnailUrl($"https://a.ppy.sh/{user.Id}")
    .Build();

  /// <summary>
  /// Returns an embed for displaying all beatmap aliases.
  /// </summary>
  /// <param name="aliases">The beatmap aliases.</param>
  public Embed BeatmapAliases(IEnumerable<BeatmapAlias> aliases)
  {
    // Display the aliases in alphabetical order.
    aliases = aliases.OrderBy(x => x.Alias);

    string aliasesStr = "";
    if (aliases.Any())
      foreach (IGrouping<int, BeatmapAlias> group in aliases.GroupBy(x => x.BeatmapId))
        aliasesStr += $"""
                       [{group.First().DisplayName}](https://osu.ppy.sh/b/{group.Key})
                       ▸ {string.Join(", ", group.Select(j => $"`{j.Alias}`"))}


                       """;

    return BaseEmbed
      .WithColor(new Color(0xF1C40F))
      .WithTitle("List of all beatmap aliases")
      .WithDescription($"""
                        *These aliases can be used in place of where you'd specify a beatmap ID in order to access those beatmaps more easily.*

                        {(aliasesStr.Length > 0 ? aliasesStr : "*There are no beatmap aliases. You can add some via `/alias beatmap add`.*")}
                        """)
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying all score aliases.
  /// </summary>
  /// <param name="aliases">The score aliases.</param>
  public Embed ScoreAliases(IEnumerable<ScoreAlias> aliases)
  {
    // Display the aliases in alphabetical order.
    aliases = aliases.OrderBy(x => x.Alias);

    string aliasesStr = "";
    if (aliases.Any())
      foreach (IGrouping<long, ScoreAlias> group in aliases.GroupBy(x => x.ScoreId))
        aliasesStr += $"""
                       [{group.First().DisplayName}](https://osu.ppy.sh/scores/{group.Key})
                       ▸ {string.Join(", ", group.Select(j => $"`{j.Alias}`"))}


                       """;

    return BaseEmbed
      .WithColor(new Color(0xF1C40F))
      .WithTitle("List of all score aliases")
      .WithDescription($"""
                        *These aliases can be used in place of where you'd specify a score ID in order to access those scores more easily.*

                        {(aliasesStr.Length > 0 ? aliasesStr : "*There are no score aliases. You can add some via `/alias score add`.*")}
                        """)
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
  public Embed EffMissCount(int combo, int maxCombo, int sliderCount, int hits, int misses, double cbmc, double fct, double emc) => BaseEmbed
    .WithColor(Color.Red)
    .WithTitle("Effective Misscount Breakdown")
    .WithDescription($"""
                      ▸ {combo}/{maxCombo}x ▸ {hits} {Emojis["100"]}{Emojis["50"]} {misses} {Emojis["miss"]} ▸ {sliderCount} {Emojis["sliders"]}
                      ```
                      combo-based misscount | {cbmc.ToString($"N{Math.Max(0, 6 - ((int)cbmc).ToString().Length)}")}
                      full-combo threshold  | {fct.ToString($"N{Math.Max(0, 6 - ((int)fct).ToString().Length)}")}
                      -------------------------------
                      effective misscount   | {emc.ToString($"N{Math.Max(0, 6 - ((int)emc).ToString().Length)}")}
                      ```
                      *The reference code can be found [here](https://github.com/ppy/osu/blob/3d569850b15ad66b3c95e009f173298d65a8e3de/osu.Game.Rulesets.Osu/Difficulty/OsuPerformanceCalculator.cs#L249).*
                      """)
    .Build();

  #endregion
}