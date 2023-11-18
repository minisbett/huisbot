using Discord.Interactions;
using huisbot.Models.Utility;
using huisbot.Services;

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
    await DeferAsync();

    // Return the list of aliases in an embed.
    await FollowupAsync(embed: Embeds.Aliases(await _persistence.GetBeatmapAliasesAsync()));
  }

  [SlashCommand("add", "Adds an alias.")]
  public async Task HandleAddAsync(
    [Summary("alias", "The alias text.")] string alias,
    [Summary("beatmapId", "The ID of the beatmap.")] int beatmapId)
  {
    await DeferAsync();
    alias = alias.ToLower();

    // Check whether the alias already exists.
    BeatmapAlias? beatmapAlias = await _persistence.GetBeatmapAliasAsync(alias);
    if (beatmapAlias is not null)
    {
      await FollowupAsync(embed: Embeds.Error($"The alias `{alias}` already exists. ([{beatmapAlias.Id}](https://osu.ppy.sh/b/{beatmapAlias.Id}))"));
      return;
    }

    // Add the alias.
    await _persistence.AddBeatmapAliasAsync(alias, beatmapId);
    await FollowupAsync(embed: Embeds.Success($"The alias `{alias}` has successfully added. ([{beatmapId}](https://osu.ppy.sh/b/{beatmapId}))"));
  }

  [SlashCommand("remove", "Adds an alias.")]
  public async Task HandleRemoveAsync(
    [Summary("alias", "The alias to remove.")] string alias)
  {
    await DeferAsync();
    alias = alias.ToLower();

    // Check whether the alias exists.
    BeatmapAlias? beatmapAlias = await _persistence.GetBeatmapAliasAsync(alias);
    if (beatmapAlias is null)
    {
      await FollowupAsync(embed: Embeds.Error($"The alias `{alias}` does not exist."));
      return;
    }

    // Remove the alias.
    await _persistence.RemoveBeatmapAliasAsync(beatmapAlias);
    await FollowupAsync(embed: Embeds.Success($"The alias `{alias}` was successfully removed."));
  }
}
