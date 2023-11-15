using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using huisbot.Models.Huis;
using huisbot.Services;

namespace huisbot.Modules;

/// <summary>
/// The interaction module for the reworks command.
/// </summary>
public class ReworksCommandModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly HuisApiService _huis;

  public ReworksCommandModule(HuisApiService huis)
  {
    _huis = huis;
  }

  [SlashCommand("reworks", "Outputs a list of all existing reworks.")]
  public async Task HandleAsync()
  {
    // Get all reworks and check whether the request was successful. If not, notify the user about an internal error.
    Rework[]? reworks = await _huis.GetReworksAsync();
    if (reworks is null)
    {
      await RespondAsync(embed: Embeds.InternalError("Failed to get the reworks from the Huis API."));
      return;
    }

    // Construct the select menu for selecting a rework.
    SelectMenuBuilder selectMenu = new SelectMenuBuilder()
      .WithCustomId("rework")
      .WithPlaceholder("Select a rework...")
      .WithMaxValues(1)
      .WithOptions(reworks.Select(x => new SelectMenuOptionBuilder(x.Name, x.Code, $"{x.Code} ({x.GetReadableReworkType()})", null, false)).ToList());

    // Show the live "rework" by default and add the select menu to the reply.
    await RespondAsync(embed: Embeds.Rework(reworks.First(x => x.Code == "live")), components: new ComponentBuilder().WithSelectMenu(selectMenu).Build());
  }
}

/// <summary>
/// The interaction module for the "rework" select menu from the <see cref="ReworksCommandModule"/> command.
/// </summary>
public class ReworksComponentModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly HuisApiService _huis;

  public ReworksComponentModule(HuisApiService huis)
  {
    _huis = huis;
  }

  /// <summary>
  /// Callback for interactions with the "rework" select menu from the <see cref="ReworksAsync"/> command.
  /// </summary>
  /// <returns></returns>
  [ComponentInteraction("rework")]
  public async Task HandleAsync(string code)
  {
    SocketMessageComponent interaction = (SocketMessageComponent)Context.Interaction;

    // Get all reworks and check whether the request was successful. If not, notify the user about an internal error.
    Rework[]? reworks = await _huis.GetReworksAsync();
    if (reworks is null)
    {
      await interaction.UpdateAsync(msg =>
      {
        msg.Embed = Embeds.InternalError("Failed to get the reworks from the Huis API.");
        msg.Components = null; // Remove the select menu if an error occured.
      });
      return;
    }

    // Show the selected rework.
    await interaction.UpdateAsync(msg =>
    {
      msg.Embed = Embeds.Rework(reworks.First(x => x.Code == code));
    });
  }
}