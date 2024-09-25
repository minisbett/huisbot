using Discord.Interactions;
using huisbot.Services;
using huisbot.Utilities;

namespace huisbot.Modules;

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
    await DeferAsync();

    // Return the info embed to the user.
    await FollowupAsync(embed: Embeds.Info(await _osu.IsV1AvailableAsync(), await _osu.IsV2AvailableAsync(), await _huis.IsAvailableAsync()));
  }
}
