using Discord.Interactions;
using huisbot.Models.Osu;
using huisbot.Models.Utility;
using huisbot.Services;

namespace huisbot.Modules.Utility;

/// <summary>
/// The interaction module for the alias group & add, remove and list subcommand, listing and modifying the different aliases.
/// </summary>
[Group("alias", "Commands for adding, removing and listing aliases.")]
public class AliasGroupModule : InteractionModuleBase<SocketInteractionContext>
{
  [Group("beatmap", "Commands for adding, removing and listing beatmap aliases.")]
  public class BeatmapAliasCommandModule : ModuleBase
  {
    private readonly PersistenceService _persistence;

    public BeatmapAliasCommandModule(OsuApiService osu, PersistenceService persistence) : base(osu: osu)
    {
      _persistence = persistence;
    }

    [SlashCommand("list", "Lists all existing beatmap aliases.")]
    public async Task HandleListAsync()
    {
      await DeferAsync();

      // Return the list of beatmap aliases in an embed.
      await FollowupAsync(embed: Embeds.BeatmapAliases(await _persistence.GetBeatmapAliasesAsync()));
    }

    [SlashCommand("add", "Adds a beatmap alias.")]
    public async Task HandleAddAsync(
      [Summary("alias", "The alias text.")] string aliasText,
      [Summary("beatmapId", "The ID of the beatmap.")] int beatmapId)
    {
      await DeferAsync();
      aliasText = new string(aliasText.ToLower().Where(x => x is not ('-' or '_' or '.' or ' ')).ToArray());

      // Make sure the user is part of the PP team.
      if (!await IsPPTeamAsync())
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      // Check whether the beatmap alias already exists.
      BeatmapAlias? alias = await _persistence.GetBeatmapAliasAsync(aliasText);
      if (alias is not null)
      {
        await FollowupAsync(embed: Embeds.Error($"The beatmap alias `{aliasText}` already exists.\n[{alias.DisplayName}](https://osu.ppy.sh/b/{alias.BeatmapId})"));
        return;
      }

      // Get the beatmap.
      OsuBeatmap? beatmap = await GetBeatmapAsync(beatmapId.ToString());
      if (beatmap is null)
        return;

      // Add the beatmap alias.
      alias = new BeatmapAlias(aliasText, beatmapId, $"{beatmap.Title} [{beatmap.Version}]");
      await _persistence.AddBeatmapAliasAsync(alias);
      await FollowupAsync(embed: Embeds.Success($"The beatmap alias `{aliasText}` has successfully added.\n[{alias.DisplayName}](https://osu.ppy.sh/b/{beatmapId})"));
    }

    [SlashCommand("remove", "Removes a beatmap alias.")]
    public async Task HandleRemoveAsync(
      [Summary("alias", "The beatmap alias to remove.")] string aliasText)
    {
      await DeferAsync();
      aliasText = new string(aliasText.ToLower().Where(x => x is not ('-' or '_' or '.' or ' ')).ToArray());

      // Make sure the user is part of the PP team.
      if (!await IsPPTeamAsync())
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      // Check whether the beatmap alias exists.
      BeatmapAlias? alias = await _persistence.GetBeatmapAliasAsync(aliasText);
      if (alias is null)
      {
        await FollowupAsync(embed: Embeds.Error($"The beatmap alias `{aliasText}` does not exist."));
        return;
      }

      // Remove the beatmap alias.
      await _persistence.RemoveBeatmapAliasAsync(alias);
      await FollowupAsync(embed: Embeds.Success($"The beatmap alias `{aliasText}` was successfully removed."));
    }

    [SlashCommand("rename", "Renames a beatmap alias.")]
    public async Task HandleRenameAsync(
      [Summary("alias", "The beatmap alias to rename.")] string aliasText,
      [Summary("newName", "The new name of the beatmap alias.")] string newName)
    {
      await DeferAsync();
      aliasText = new string(aliasText.ToLower().Where(x => x is not ('-' or '_' or '.' or ' ')).ToArray());
      newName = new string(newName.ToLower().Where(x => x is not ('-' or '_' or '.' or ' ')).ToArray());

      // Make sure the user is part of the PP team.
      if (!await IsPPTeamAsync())
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      // Check whether the beatmap alias exists.
      BeatmapAlias? alias = await _persistence.GetBeatmapAliasAsync(aliasText);
      if (alias is null)
      {
        await base.FollowupAsync(embed: Embeds.Error($"The beatmap alias `{alias}` does not exist."));
        return;
      }

      // Check whether the new name is already taken.
      BeatmapAlias? _alias = await _persistence.GetBeatmapAliasAsync(newName);
      if (_alias is not null)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{newName}` already exists.\n[{_alias.DisplayName}](https://osu.ppy.sh/b/{_alias.BeatmapId})"));
        return;
      }

      // Remove the beatmap alias and add the new one.
      await _persistence.RemoveBeatmapAliasAsync(alias);
      alias.Alias = newName;
      await _persistence.AddBeatmapAliasAsync(alias);
      await base.FollowupAsync(embed: Embeds.Success($"The beatmap alias `{aliasText}` has been renamed to `{newName}`."));
    }
  }

  [Group("score", "Commands for adding, removing and listing score aliases.")]
  public class ScoreAliasCommandModule : ModuleBase
  {
    private readonly PersistenceService _persistence;

    public ScoreAliasCommandModule(OsuApiService osu, PersistenceService persistence) : base(osu: osu)
    {
      _persistence = persistence;
    }

    [SlashCommand("list", "Lists all existing score aliases.")]
    public async Task HandleListAsync()
    {
      await DeferAsync();

      // Return the list of score aliases in an embed.
      await FollowupAsync(embed: Embeds.ScoreAliases(await _persistence.GetScoreAliasesAsync()));
    }

    [SlashCommand("add", "Adds a score alias.")]
    public async Task HandleAddAsync(
      [Summary("alias", "The alias text.")] string aliasText,
      [Summary("scoreId", "The ID of the beatmap.")] long scoreId)
    {
      await DeferAsync();
      aliasText = new string(aliasText.ToLower().Where(x => x is not ('-' or '_' or '.' or ' ')).ToArray());

      // Make sure the user is part of the PP team.
      if (!await IsPPTeamAsync())
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      // Check whether the score alias already exists.
      ScoreAlias? alias = await _persistence.GetScoreAliasAsync(aliasText);
      if (alias is not null)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{aliasText}` already exists.\n[{alias.DisplayName}](https://osu.ppy.sh/scores/osu/{alias.ScoreId})"));
        return;
      }

      // Get the score.
      OsuScore? score = await GetScoreAsync(0 /* TODO: Support for other rulsets */, scoreId.ToString());
      if (score is null)
        return;

      // Add the score alias.
      alias = new ScoreAlias(aliasText, scoreId, $"{score.User.Name} on {score.BeatmapSet.Title} [{score.Beatmap.Version}]");
      await _persistence.AddScoreAliasAsync(alias);
      await FollowupAsync(embed: Embeds.Success($"The score alias `{aliasText}` has successfully added.\n[{alias.DisplayName}](https://osu.ppy.sh/scores/osu/{scoreId})"));
    }

    [SlashCommand("remove", "Removes a score alias.")]
    public async Task HandleRemoveAsync(
      [Summary("alias", "The score alias to remove.")] string aliasText)
    {
      await DeferAsync();
      aliasText = new string(aliasText.ToLower().Where(x => x is not ('-' or '_' or '.' or ' ')).ToArray());

      // Make sure the user is part of the PP team.
      if (!await IsPPTeamAsync())
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      // Check whether the score alias exists.
      ScoreAlias? alias = await _persistence.GetScoreAliasAsync(aliasText);
      if (alias is null)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{aliasText}` does not exist."));
        return;
      }

      // Remove the score alias.
      await _persistence.RemoveScoreAliasAsync(alias);
      await FollowupAsync(embed: Embeds.Success($"The score alias `{aliasText}` was successfully removed."));
    }

    [SlashCommand("rename", "Renames a score alias.")]
    public async Task HandleRenameAsync(
      [Summary("alias", "The score alias to rename.")] string aliasText,
      [Summary("newName", "The new name of the score alias.")] string newName)
    {
      await DeferAsync();
      aliasText = new string(aliasText.ToLower().Where(x => x is not ('-' or '_' or '.' or ' ')).ToArray());
      newName = new string(newName.ToLower().Where(x => x is not ('-' or '_' or '.' or ' ')).ToArray());

      // Make sure the user is part of the PP team.
      if (!await IsPPTeamAsync())
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      // Check whether the score alias exists.
      ScoreAlias? alias = await _persistence.GetScoreAliasAsync(aliasText);
      if (alias is null)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{alias}` does not exist."));
        return;
      }

      // Check whether the new name is already taken.
      ScoreAlias? _alias = await _persistence.GetScoreAliasAsync(newName);
      if (_alias is not null)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{newName}` already exists.\n[{_alias.DisplayName}](https://osu.ppy.sh/scores/osu/{_alias.ScoreId})"));
        return;
      }

      // Remove the score alias and add the new one.
      await _persistence.RemoveScoreAliasAsync(alias);
      alias.Alias = newName;
      await _persistence.AddScoreAliasAsync(alias);
      await base.FollowupAsync(embed: Embeds.Success($"The score alias `{aliasText}` has been renamed to `{newName}`."));
    }
  }
}
