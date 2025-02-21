using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the user command, displaying info about a user in a rework.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public class ProfileCommandModule(IServiceProvider services) : ModuleBase(services)
{
  [SlashCommand("profile", "Displays info about you or the specified user in the specified rework.")]
  public async Task HandleAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("user", "The osu! ID or name of the user. Optional, defaults to your linked osu! user.")] string? userId = null)
  {
    await DeferAsync();

    // If no user identifier was specified, try to get one from an osu-discord link.
    if (userId is null)
      if (await GetOsuDiscordLinkAsync() is OsuDiscordLink link)
        userId = link.OsuId.ToString();
      else
        return;

    // The live rework is required to determine whether the user is outdated in it, causing an inproper comparison.
    if (await GetReworkAsync(reworkId) is not HuisRework local) return;
    if (await GetReworkAsync(HuisRework.LiveId.ToString()) is not HuisRework live) return;
    if (await GetOsuUserAsync(userId) is not OsuUser user) return;
    if (await GetHuisPlayerAsync(user, local) is not HuisPlayer localUser) return;
    if (await GetHuisPlayerAsync(user, live) is not HuisPlayer liveUser) return;

    // Show the user embed.
    await FollowupAsync(embed: Embeds.Player(localUser, liveUser, local));
  }
}