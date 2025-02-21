using Discord;
using Discord.Interactions;
using huisbot.Helpers;
using huisbot.Models.Huis;
using huisbot.Services;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the rankings group & player and score subcommand, displaying the global leaderboard in a rework.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
[Group("rankings", "Commands for the global player/score rankings of a rework.")]
public class RankingsCommandModule(IServiceProvider services) : ModuleBase(services)
{
  /// <summary>
  /// Represents the cached score values for providing pagination via Discord message components.
  /// This prevents those values from having to be fetched everytime the page is switched.
  /// </summary>
  private record ScorePaginationCacheEntry(HuisScore[] Scores, HuisRework Rework, Sort Sort);

  /// <summary>
  /// Represents the cached player values for providing pagination via Discord message components.
  /// This prevents those values from having to be fetched everytime the page is switched.
  /// </summary>
  private record PlayerPaginationCacheEntry(HuisPlayer[] Players, HuisRework Rework, Sort Sort);

  /// <summary>
  /// A dictionary of entries of cached score values for providing pagination via Discord message components with their unique ID.
  /// </summary>
  private static readonly Dictionary<string, ScorePaginationCacheEntry> _scorePaginationCache = [];

  /// <summary>
  /// A dictionary of entries of cached player values for providing pagination via Discord message components with their unique ID.
  /// </summary>
  private static readonly Dictionary<string, PlayerPaginationCacheEntry> _playerPaginationCache = [];

  [SlashCommand("player", "Displays the global player leaderboard of the specified rework.")]
  public async Task HandlePlayerAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("sort", "The sorting for the players. Defaults to sort by Local PP.")]
    [Autocomplete(typeof(RankingPlayersSortAutocomplete))] string sortId = "new_pp_incl_bonus_desc",
    [Summary("onlyUpToDate", "Bool whether outdated players/uncalculated will be included. Defaults to true.")] bool onlyUpToDate = true,
    [Summary("hideUnranked", "Bool whether inactive players should be excluded. Defaults to true.")] bool hideUnranked = true,
    [Summary("page", "The page of the players. 1 page displays 10 players.")][MinValue(1)] int page = 1)
  {
    await DeferAsync();

    if (await GetSortAsync(sortId, Sort.RankingPlayers) is not Sort sort) return;
    if (await GetReworkAsync(reworkId) is not HuisRework rework) return;
    if (await GetPlayerRankingsAsync(rework.Id, sort, onlyUpToDate, hideUnranked) is not HuisPlayer[] players) return;

    // Cache the results and build a message component for pagination navigation.
    string cacheId = Guid.NewGuid().ToString();
    _playerPaginationCache[cacheId] = new PlayerPaginationCacheEntry(players, rework, sort);
    int maxPage = (int)Math.Ceiling(players.Length * 1d / EmbedService.SCORES_PER_PAGE);
    ComponentBuilder builder = new ComponentBuilder()
      .WithButton("←", $"rankings_player:page:{cacheId},{page - 1}", ButtonStyle.Secondary, disabled: page == 1)
      .WithButton("→", $"rankings_player:page:{cacheId},{page + 1}", ButtonStyle.Secondary, disabled: page == maxPage);

    await FollowupAsync(embed: Embeds.PlayerRankings(players, rework, sort, page), components: builder.Build());
  }

  [SlashCommand("score", "Displays the global score leaderboard of the specified rework.")]
  public async Task HandleScoreAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("sort", "The sorting for the scores. Defaults to sort by Local PP.")]
    [Autocomplete(typeof(RankingScoresSortAutocomplete))] string sortId = "local_pp_desc",
    [Summary("page", "The page of the scores. 1 page displays 10 scores.")][MinValue(1)] int page = 1)
  {
    await DeferAsync();

    if (await GetSortAsync(sortId, Sort.RankingPlayers) is not Sort sort) return;
    if (await GetReworkAsync(reworkId) is not HuisRework rework) return;
    if (await GetScoreRankingsAsync(rework.Id, sort) is not HuisScore[] scores) return;

    // Cache the results and build a message component for pagination navigation.
    string cacheId = Guid.NewGuid().ToString();
    _scorePaginationCache[cacheId] = new ScorePaginationCacheEntry(scores, rework, sort);
    int maxPage = (int)Math.Ceiling(scores.Length * 1d / EmbedService.SCORES_PER_PAGE);
    ComponentBuilder builder = new ComponentBuilder()
      .WithButton("←", $"rankings_score:page:{cacheId},{page - 1}", ButtonStyle.Secondary, disabled: page == 1)
      .WithButton("→", $"rankings_score:page:{cacheId},{page + 1}", ButtonStyle.Secondary, disabled: page == maxPage);

    await FollowupAsync(embed: Embeds.ScoreRankings(scores, rework, sort, page), components: builder.Build());
  }

  [ComponentInteraction("rankings_score:page:*,*", ignoreGroupNames: true)]
  public async Task HandleScorePageAsync(string cacheId, int page)
  {
    await DeferAsync();

    // Get the corresponding cache entry of the message.
    IUserMessage msg = (Context.Interaction as IComponentInteraction)!.Message;
    ScorePaginationCacheEntry entry = _scorePaginationCache[cacheId];

    // Re-build the message component for further pagination navigation.
    int maxPage = (int)Math.Ceiling(entry.Scores.Length * 1d / EmbedService.SCORES_PER_PAGE);
    ComponentBuilder builder = new ComponentBuilder()
      .WithButton("←", $"rankings_score:page:{cacheId},{page - 1}", ButtonStyle.Secondary, disabled: page == 1)
      .WithButton("→", $"rankings_score:page:{cacheId},{page + 1}", ButtonStyle.Secondary, disabled: page == maxPage);

    // Update the embed with the values of the requested page.
    await msg.ModifyAsync(x =>
    {
      x.Embed = Embeds.ScoreRankings(entry.Scores, entry.Rework, entry.Sort, page);
      x.Components = builder.Build();
    });
  }

  [ComponentInteraction("rankings_player:page:*,*", ignoreGroupNames: true)]
  public async Task HandlePlayerPageAsync(string cacheId, int page)
  {
    await DeferAsync();

    // Get the corresponding cache entry of the message.
    IUserMessage msg = (Context.Interaction as IComponentInteraction)!.Message;
    PlayerPaginationCacheEntry entry = _playerPaginationCache[cacheId];

    // Re-build the message component for further pagination navigation.
    int maxPage = (int)Math.Ceiling(entry.Players.Length * 1d / EmbedService.PLAYERS_PER_PAGE);
    ComponentBuilder builder = new ComponentBuilder()
      .WithButton("←", $"rankings_player:page:{cacheId},{page - 1}", ButtonStyle.Secondary, disabled: page == 1)
      .WithButton("→", $"rankings_player:page:{cacheId},{page + 1}", ButtonStyle.Secondary, disabled: page == maxPage);

    // Update the embed with the values of the requested page.
    await msg.ModifyAsync(x =>
    {
      x.Embed = Embeds.PlayerRankings(entry.Players, entry.Rework, entry.Sort, page);
      x.Components = builder.Build();
    });
  }

  /// <summary>
  /// Autocomplete for the sort parameter on the score rankings command.
  /// </summary>
  private class RankingScoresSortAutocomplete : AutocompleteHandler
  {
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction acInteraction,
      IParameterInfo pInfo, IServiceProvider services)
        => Task.FromResult(AutocompletionResult.FromSuccess(Sort.RankingScores.Select(x => new AutocompleteResult(x.DisplayName, x.Id))));
  }

  /// <summary>
  /// Autocomplete for the sort parameter on the player rankings command.
  /// </summary>
  private class RankingPlayersSortAutocomplete : AutocompleteHandler
  {
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction acInteraction,
      IParameterInfo pInfo, IServiceProvider services)
        => Task.FromResult(AutocompletionResult.FromSuccess(Sort.RankingPlayers.Select(x => new AutocompleteResult(x.DisplayName, x.Id))));
  }
}
