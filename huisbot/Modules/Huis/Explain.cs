using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Services;
using huisbot.Utilities.Discord;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.Diagnostics;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the explain command, explaining the changes of a rework.
/// </summary>
public class ExplainCommandModule : ModuleBase
{
  private readonly OpenAIClient _openAI;
  private readonly HttpClient _http = new HttpClient();
  private readonly ILogger<ExplainCommandModule> _logger;

  /// <summary>
  /// The ID of the OpenAI model used.
  /// </summary>
  private const string OPENAI_MODEL = "gpt-4o-mini";

  /// <summary>
  /// The cents spent per input token for the OpenAI chat completion.
  /// </summary>
  private const double CENTS_PER_INPUT_TOKEN = 0.000015;

  /// <summary>
  /// The cents spent per output token for the OpenAI chat completion.
  /// </summary>
  private const double CENTS_PER_OUTPUT_TOKEN = 0.000060;

  /// <summary>
  /// The system message for the OpenAI chat completion.
  /// </summary>
  private const string SYSTEM_MESSAGE =
    """
    You will be provided a git diff patch. It represents the changes made to the performance points and difficulty calculation algorithm
    for the rhythm game osu!. You will not tell the user about that you received this patch file or anything, you just follow your job.
    Your job is to explain what effect the changes made has on the algorithm as a whole, and maybe also how it
    would shift the meta of the game.
    
    The code changes itself are uninteresting to the user, so ONLY name the effects of the changes and what they effectively mean.
    Try to include code snippets for all changes you explain. Stay concise and to the point.
    Note that the code/rework in question is a proposal and not any implemented changes. Never talk about "recent changes".
    Those are not implemented changes, they are just proposals.

    The user may not know the codebase or programming general, so if you use code snippets for reference do not assume much prior knowledge.
    Do not do unreliable assumptions about osu! and the current meta of the game, if you don't know just leave it out.
    If terms come up that you think are not known to the user, you can explain them in a simple way. Do not assume the user knows them.
    But also do not guess the meaning of the terms, if you don't know just leave it out.

    Format the response for Discord, since you are a Discord bot that responds to a command requesting an analysis of a rework.
    You cannot use markdown headers such as "# Title" or "## Subtitle", as the response will be displayed in an embed.
    Bold, italic and code formatting is supported.

    Include the source of ALL code snippets. No exception. Put a hyperlink by the cited code snippet. You can get URLs to the source files
    based on the commit URL provided in the rework information of the first user message. You can tell which lines the change happens in
    by the patch file you received. Here's an example for a commit URL: https://github.com/<user>/<repo>/tree/<commit>
    Based on that you can generate hyperlinks such like [source](https://github.com/<user>/<repo>/blob/<commit>/path/to/a/file.cs#L1-L5)

    Do NOT start with an introduction that mentions this is an analysis (that's obvious from the context), and also do not say that
    the changes affect the performance points and difficulty calculation algorithm for the rhythm game osu!, as that is also obvious.
    Just go straight to the analysis instead. We don't need some introduction saying such stuff, because it's obvious from the context.
    
    Do not greet the user or provide any additional information, just perform the analysis of the changes in the patch.
    """;

  public ExplainCommandModule(HuisApiService huis, OpenAIClient openAI, ILogger<ExplainCommandModule> logger) : base(huis)
  {
    _openAI = openAI;
    _logger = logger;
  }

  [SlashCommand("explain", "Explains the changes made in the specified rework.")]
  public async Task HandleScoreAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId)
  {
    await DeferAsync();

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // Check if the rework has the source code available.
    if (rework.CommitUrl is null)
    {
      await FollowupAsync(embed: Embeds.Error("The source code for this rework is not available."));
      return;
    }

    // Notify the user that analysis is being performed.
    IUserMessage msg = await FollowupAsync(embed: Embeds.AnalysisRunning(rework));

    // Get the patch data from the rework compared to the ppy/osu master branch.
    string patch = await _http.GetStringAsync($"https://github.com/ppy/osu/compare/master...{rework.GitHubUrl!.Split('/')[3]}:{rework.Commit}.patch");

    // Generate the AI response.
    Stopwatch watch = Stopwatch.StartNew();
    ChatCompletion completion = (await _openAI.GetChatClient(OPENAI_MODEL).CompleteChatAsync(
      new SystemChatMessage(SYSTEM_MESSAGE),
      new UserChatMessage(
       $"""
        Here are some general information about the rework:
        Name: {rework.Name}
        Ruleset: {rework.RulesetName}"
        Description: {rework.Description ?? "No description available."}
        Commit Url: {rework.CommitUrl}
        """),
      new UserChatMessage(
       $"""
        Here is the .patch file:
        ```diff
        {patch}
        ```
        """)
    )).Value;
    watch.Stop();

    // Send the analysis response to the user.
    await ModifyOriginalResponseAsync(msg => msg.Embed = Embeds.Analysis(rework, string.Join("", completion.Content.Select(x => x.Text))));

    // Output information about the token usage and the time taken for the AI generation to the logger.
    _logger.LogInformation("Token Usage: {Total:N0} ({Input:N0} input, {Output:N0} output) @ {Cost} cent ({InputCost} input, {OutputCost} output)",
      completion.Usage.TotalTokens, completion.Usage.InputTokens, completion.Usage.OutputTokens,
      completion.Usage.InputTokens * CENTS_PER_INPUT_TOKEN + completion.Usage.OutputTokens * CENTS_PER_OUTPUT_TOKEN,
      completion.Usage.InputTokens * CENTS_PER_INPUT_TOKEN, completion.Usage.OutputTokens * CENTS_PER_OUTPUT_TOKEN);

    _logger.LogInformation("AI Generation took {Time}s", Math.Round(watch.Elapsed.TotalSeconds, 1));
  }
}