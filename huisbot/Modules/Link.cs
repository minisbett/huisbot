using Discord;
using Discord.Interactions;
using huisbot.Models.Osu;
using huisbot.Services;
using huisbot.Utilities;

namespace huisbot.Modules;

/// <summary>
/// The interaction module for the link command, linking an osu! username to a Discord user.
/// </summary>

[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public class LinkCommandModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly OsuApiService _osu;
  private readonly PersistenceService _links;

  public LinkCommandModule(OsuApiService osuApi, PersistenceService links)
  {
    _osu = osuApi;
    _links = links;
  }

  [SlashCommand("link", "Links your Discord account to the specified osu! user by it's ID or name.")]
  public async Task LinkAsync(
    [Summary("user", "The osu! ID or name of the player.")] string userId)
  {
    await DeferAsync();

    // Get the user from the osu! api. If it failed or the user could not be found, notify the user.
    NotFoundOr<OsuUser>? user = await _osu.GetUserAsync(userId);
    if (user is null)
    {
      await FollowupAsync(embed: Embeds.InternalError("Failed to resolve the user from the osu! API."));
      return;
    }
    else if (!user.Found)
    {
      await FollowupAsync(embed: Embeds.Error($"No player with identifier `{userId}` could not be found."));
      return;
    }

    // Otherwise add/update the link in the database and notify the user about the change.
    await _links.SetOsuDiscordLinkAsync(Context.User.Id, ((OsuUser)user).Id);
    await FollowupAsync(embed: Embeds.LinkSuccessful(user));
  }
}