using Discord.Interactions;
using huisbot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Modules.Utility;

/// <summary>
/// The interaction module for the alias group & add, remove and list subcommand, listing and modifying the beatmap aliases.
/// </summary>
[Group("alias", "Commands for adding, removing and listing beatmap aliases.")]
public class AliasCommandModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly PersistenceService _persistence;

  public AliasCommandModule(PersistenceService persistence)
  {
    _persistence = persistence;
  }

  [SlashCommand("list", "Lists all existing beatmap aliases.")]
  public async Task HandleListAsync()
  {
    // Return the list of aliases in an embed.
    await RespondAsync(embed: Embeds.Aliases(await _persistence.GetBeatmapAliasesAsync()));
  }
}
