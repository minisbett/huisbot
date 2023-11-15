using Discord;
using huisbot.Extensions;
using huisbot.Models.Huis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
  /// Returns an error embed for displaying an error message.
  /// </summary>
  /// <param name="message">The error message.</param>
  /// <returns>An embed for displaying an error message.</returns>
  public static Embed Error(string message) => BaseEmbed
    .WithColor(Color.Red)
    .WithDescription(message)
    .Build();

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
  /// Returns an embed with the specified rework.
  /// </summary>
  /// <param name="rework">The rework to display.</param>
  /// <returns>The embed for displaying the specified rework.</returns>
  public static Embed Rework(Rework rework)
  {
    return BaseEmbed
      .WithTitle($"{rework.Id} {rework.Name} ({rework.Code})")
      .WithUrl($"https://pp.huismetbenen.nl/rankings/info/{rework.Code}")
      .AddField("Description", (rework.Description ?? "") == "" ? "*No description available.*" : rework.Description)
      .AddField("Ruleset", rework.GetReadableRuleset(), true)
      .AddField("Links", $"[Huismetbenen](https://pp.huismetbenen.nl/rankings/info/{rework.Code}) • [Source Code]({rework.GetBranchUrl()})", true)
      .AddField("Status", rework.GetReadableReworkType(), true)
      .Build();
  }

  /// <summary>
  /// Returns an embed for displaying the specified player in the specified rework.
  /// </summary>
  /// <param name="player">The player to display.</param>
  /// <param name="rework">The rework.</param>
  /// <returns></returns>
  public static Embed Player(Player player, Rework rework)
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
      .AddField("**Useful Links**", $"[osu! profile](https://osu.ppy.sh/u/{player.Id}) • {huisRework} • {huisProfile} • [Source Code]({rework.GetBranchUrl()})")
      .WithFooter($"{BaseEmbed.Footer} • Last Updated")
      .WithTimestamp(player.LastUpdated)
      .Build();
  }
}
