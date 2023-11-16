using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the leaderboard group & player and score subcommand, displaying the global leaderboard in a rework.
/// </summary>
[Group("leaderboard", "Commands for the global player/score leaderboard of a rework.")]
public class LeaderboardCommandModule : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand("player", "Displays the global player leaderboard of the specified rework.")]
  public async Task HandlePlayerAsync()
  {

  }

  [SlashCommand("score", "Displays the global score leaderboard of the specified rework.")]
  public async Task HandleScoreAsync()
  {

  }
}
