using Discord;
using Discord.Interactions;
using huisbot.Services;
using huisbot.Utilities;

namespace huisbot.Modules;

/// <summary>
/// The interaction module for the info command, displaying general info about the bot.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public class InfoCommandModule(OsuApiService osu, HuisApiService huis) : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand("info", "Displays info about the bot.")]
  public async Task HandleAsync()
  {
    await DeferAsync();

    // Return the info embed to the user.
    await FollowupAsync(embed: Embeds.Info(await osu.IsV1AvailableAsync(), await osu.IsV2AvailableAsync(), await huis.IsAvailableAsync()));
  }
}
