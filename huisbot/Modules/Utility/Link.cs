using Discord.Interactions;
using huisbot.Models.Osu;
using huisbot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Modules.Utility;

/// <summary>
/// The interaction module for the link command, linking an osu! username to a Discord user.
/// </summary>
public class LinkCommandModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly OsuApiService _osu;
  private readonly OsuDiscordLinkService _links;

  public LinkCommandModule(OsuApiService osuApi, OsuDiscordLinkService links)
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
    OsuUser? user = await _osu.GetUserAsync(userId);
    if (user is null)
    {
      await FollowupAsync(embed: Embeds.InternalError("Failed to resolve the user from the osu! API."));
      return;
    }
    else if (!user.WasFound)
    {
      await FollowupAsync(embed: Embeds.Error($"No player with identifier `{userId}` could not be found."));
      return;
    }

    // Otherwise add/update the link in the database and notify the user about the change.
    await _links.SetLinkAsync(Context.User.Id, user.Id);
    await FollowupAsync(embed: Embeds.LinkSuccessful(user));
  }
}