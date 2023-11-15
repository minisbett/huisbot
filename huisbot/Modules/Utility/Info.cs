using Discord.Interactions;
using huisbot.Modules.Huis;
using huisbot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Modules.Utility;

/// <summary>
/// The interaction module for the info command, displaying general info about the bot.
/// </summary>
public class InfoCommandModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly OsuApiService _osu;
  private readonly HuisApiService _huis;

  public InfoCommandModule(OsuApiService osu, HuisApiService huis)
  {
    _osu = osu;
    _huis = huis;
  }

  [SlashCommand("info", "Displays info about the bot.")]
  public async Task HandleAsync()
  {
    // Return the info embed to the user.
    await RespondAsync(embed: Embeds.Info(await _osu.IsAvailableAsync(), await _huis.IsAvailableAsync()));
  }
}
