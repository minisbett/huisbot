using Discord.Interactions;
using huisbot.Modules.Autocompletes;
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
  public async Task HandleScoreAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("sort", "The sorting and order for the scores.")] string sort)
    //[Choice string sort)
  {

  }
}
