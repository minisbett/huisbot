using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using huisbot.Models.Huis;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the reworks command, displaying info about all reworks.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public class ReworksCommandModule(IServiceProvider services) : ModuleBase(services)
{
  [SlashCommand("reworks", "Outputs a list of all existing reworks.")]
  public async Task HandleAsync()
  {
    await DeferAsync();

    if (await GetReworksAsync() is not HuisRework[] reworks) return;

    // Strip off all Onion-level reworks if the user does not have Onion-authorization.
    if (!IsOnion)
      reworks = [.. reworks.Where(x => !x.IsOnionOnly)];

    // Filter out some rather uninteresting reworks if more than 25 reworks exist, as only up to 25 items can be displayed in a select menu.
    if (reworks.Length > 25)
      reworks = [.. reworks.Where(x => !x.IsHistoric)];
    if (reworks.Length > 25)
      reworks = [.. reworks.Where(x => !x.IsAbandoned)];
    if (reworks.Length > 25)
      reworks = [.. reworks.Take(25)]; // As a "last resort", limit the reworks to 25

    MessageComponent component = new ComponentBuilder()
      .WithSelectMenu(new SelectMenuBuilder()
        .WithCustomId("rework")
        .WithPlaceholder("Select a rework...")
        .WithMaxValues(1)
        .WithOptions(reworks.Select(x => new SelectMenuOptionBuilder(x.Name, x.Code, $"{x.Code} ({x.ReworkTypeString} )", null, false)).ToList()))
      .Build();

    // Show the live "rework" by default.
    await FollowupAsync(embed: Embeds.Rework(reworks.First(x => x.IsLive)), components: component);
  }

  [ComponentInteraction("rework")]
  public async Task HandleReworkInteractionAsync(string code)
  {
    if ((await GetReworksAsync())?.FirstOrDefault(x => x.Code == code) is not HuisRework rework) return;

    // Block this interaction if the selected rework is Onion-level and the user does not have Onion-authorization.
    // This can happen if a non-Onion user tries to interact with a select menu created by a user with Onion-authorization.
    if (rework.IsOnionOnly && !IsOnion)
    {
      await Context.Interaction.RespondAsync(embed: Embeds.NotOnion, ephemeral: true);
      return;
    }

    await ((SocketMessageComponent)Context.Interaction).UpdateAsync(msg => msg.Embed = Embeds.Rework(rework));
  }
}