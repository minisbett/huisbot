using Discord;
using Discord.Interactions;

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

    bool osuApiV2 = await OsuApi.IsV2AvailableAsync();
    bool huisApi = await HuisApi.IsAvailableAsync();

    (int guildInstalls, int userInstalls) = await Discord.GetInstallCountsAsync();

    await FollowupAsync(embed: Embeds.Info(osuApiV2, huisApi, guildInstalls, userInstalls));
  }
}
