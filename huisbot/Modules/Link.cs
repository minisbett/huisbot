using Discord;
using Discord.Interactions;
using huisbot.Models.Osu;

namespace huisbot.Modules;

/// <summary>
/// The interaction module for the link command, linking an osu! username to a Discord user.
/// </summary>

[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public class LinkCommandModule(IServiceProvider services) : ModuleBase(services)
{
  [SlashCommand("link", "Links your Discord account to the specified osu! user by it's ID or name.")]
  public async Task LinkAsync(
    [Summary("user", "The osu! ID or name of the user.")] string userId)
  {
    await DeferAsync();

    if (await GetOsuUserAsync(userId) is not OsuUser user) return;

    await Persistence.SetOsuDiscordLinkAsync(Context.User.Id, user.Id);
    await FollowupAsync(embed: Embeds.LinkSuccessful(user));
  }
}