using Discord;
using Discord.Interactions;
using huisbot.Services;
using Microsoft.Extensions.Configuration;

namespace huisbot.Modules;

/// <summary>
/// The interaction module for the info command, displaying general info about the bot.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public class InfoCommandModule(IServiceProvider services) : ModuleBase(services)
{
  [SlashCommand("info", "Displays info about the bot.")]
  public async Task HandleAsync()
  {
    await DeferAsync();

    // Get the availability state of the APIs.
    bool osuApiV1 = await OsuApi.IsV1AvailableAsync();
    bool osuApiV2 = await OsuApi.IsV2AvailableAsync();
    bool huisApi = await HuisApi.IsAvailableAsync();

    // Get the installation counts for guilds and users.
    (int guildInstalls, int userInstalls) = await Discord.GetInstallCountsAsync();

    // Return the info embed to the user.
    await FollowupAsync(embed: Embeds.Info(osuApiV1, osuApiV2, huisApi, guildInstalls, userInstalls));
  }
}
