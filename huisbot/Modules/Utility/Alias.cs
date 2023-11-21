using Discord.Interactions;
using huisbot.Models.Osu;
using huisbot.Models.Utility;
using huisbot.Services;

namespace huisbot.Modules.Utility;

/// <summary>
/// The interaction module for the alias group & add, remove and list subcommand, listing and modifying the beatmap aliases.
/// </summary>
[Group("alias", "Commands for adding, removing and listing beatmap aliases.")]
public class AliasCommandModule : ModuleBase
{
  private readonly PersistenceService _persistence;

  public AliasCommandModule(HuisApiService huis, OsuApiService osu, PersistenceService persistence) : base(huis, osu)
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
    [Summary("alias", "The alias text.")] string aliasText,
    [Summary("beatmapId", "The ID of the beatmap.")] int beatmapId)
  {
    await DeferAsync();
    aliasText = aliasText.ToLower().Replace("-", "");

    // Check whether the alias already exists.
    BeatmapAlias? alias = await _persistence.GetBeatmapAliasAsync(aliasText);
    if (alias is not null)
    {
      await FollowupAsync(embed: Embeds.Error($"The alias `{aliasText}` already exists.\n[{alias.DisplayName}](https://osu.ppy.sh/b/{alias.Id})"));
      return;
    }

    // Get the beatmap.
    OsuBeatmap? beatmap = await GetBeatmapAsync(beatmapId.ToString());
    if (beatmap is null)
      return;

    // Add the alias.
    alias = new BeatmapAlias(aliasText, beatmapId, $"{beatmap.Title} [{beatmap.Version}]");
    await _persistence.AddBeatmapAliasAsync(alias);
    await FollowupAsync(embed: Embeds.Success($"The alias `{aliasText}` has successfully added.\n[{alias.DisplayName}](https://osu.ppy.sh/b/{beatmapId})"));
  }

  [SlashCommand("remove", "Removes an alias.")]
  public async Task HandleRemoveAsync(
    [Summary("alias", "The alias to remove.")] string aliasText)
  {
    await DeferAsync();
    aliasText = aliasText.ToLower().Replace("-", "");

    // Check whether the alias exists.
    BeatmapAlias? alias = await _persistence.GetBeatmapAliasAsync(aliasText);
    if (alias is null)
    {
      await FollowupAsync(embed: Embeds.Error($"The alias `{aliasText}` does not exist."));
      return;
    }

    // Remove the alias.
    await _persistence.RemoveBeatmapAliasAsync(alias);
    await FollowupAsync(embed: Embeds.Success($"The alias `{aliasText}` was successfully removed."));
  }

  [SlashCommand("rename", "Renames an alias.")]
  public async Task HandleRenameAsync(
    [Summary("alias", "The alias to rename.")] string aliasText,
    [Summary("newName", "The new name of the alias.")] string newName)
  {
    await DeferAsync();
    aliasText = aliasText.ToLower().Replace("-", "");
    newName = newName.ToLower().Replace("-", "");

    // Check whether the alias exists.
    BeatmapAlias? alias = await _persistence.GetBeatmapAliasAsync(aliasText);
    if (alias is null)
    {
      await base.FollowupAsync(embed: Embeds.Error($"The alias `{alias}` does not exist."));
      return;
    }

    // Remove the alias and add the new one.
    await _persistence.RemoveBeatmapAliasAsync(alias);
    alias.Alias = newName;
    await _persistence.AddBeatmapAliasAsync(alias);
    await base.FollowupAsync(embed: Embeds.Success($"The alias `{alias}` was renamed to `{newName}`."));
  }
}
