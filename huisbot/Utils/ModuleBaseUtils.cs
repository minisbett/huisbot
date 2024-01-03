using Discord.Interactions;
using Discord.WebSocket;
using huisbot.Modules;

namespace huisbot.Utils;

/// <summary>
/// Provides utility methods for the <see cref="ModuleBase"/>, which are included here since they may also be used else-where.
/// </summary>
public static class ModuleBaseUtils
{
  /// <summary>
  /// Bool whether the user has the Onion role on the PP Discord, making them eligible to use Huis commands.
  /// </summary>
  public static async Task<bool> IsOnionAsync(SocketInteractionContext context)
  {
#if DEBUG
    return false;
#endif

    // Check whether the user is the owner of the application.
    if (context.User.Id == (await context.Client.GetApplicationInfoAsync()).Owner.Id)
      return true;

    // Get the PP Discord guild.
    SocketGuild guild = context.Client.GetGuild(546120878908506119);

    // Check whether the user is in that guild and has the Onion role.
    SocketGuildUser user = guild.GetUser(context.User.Id);
    return user != null && user.Roles.Any(x => x.Id == 577267917662715904);
  }

  /// <summary>
  /// Returns whether the user has the PP role on the PP Discord, making them eligible for certain more critical commands.
  /// </summary>
  public static async Task<bool> IsPPTeamAsync(SocketInteractionContext context)
  {
#if DEBUG
    return true;
#endif

    // Check whether the user is the owner of the application.
    if (context.User.Id == (await context.Client.GetApplicationInfoAsync()).Owner.Id)
      return true;

    // Get the PP Discord guild.
    SocketGuild guild = context.Client.GetGuild(546120878908506119);

    // Check whether the user is in that guild and has the PP role.
    SocketGuildUser user = guild.GetUser(context.User.Id);
    return user != null && user.Roles.Any(x => x.Id == 975402380411666482);
  }
}
