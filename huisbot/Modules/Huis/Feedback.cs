using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;
using huisbot.Services;
using huisbot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Modules.Huis;

public class FeedbackCommandModule : ModuleBase
{
  public FeedbackCommandModule(HuisApiService huis) : base(huis) { }

  [SlashCommand("feedback", "Feedback")]
  public async Task HandleAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId)
  {
    // Get the specified rework.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // Make sure the rework is eligible.
    if(rework.IsLive || !rework.IsActive || rework.IsHistoric || rework.IsConfirmed)
    {
      await RespondAsync(embed: Embeds.Error("The specified rework cannot receive feedback."));
      return;
    }

    // Build the modal and respond.
    await RespondWithModalAsync<FeedbackModal>($"pp_feedback_{rework.Id}");
  }
}

/// <summary>
/// The interaction module for the "rework" select menu from the <see cref="ReworksCommandModule"/> command.
/// </summary>
public class FeedbackModalModule : ModuleBase
{
  public FeedbackModalModule(HuisApiService huis) : base(huis) { }

  [ModalInteraction("pp_feedback_.*", TreatAsRegex = true)]
  public async Task HandleAsync(FeedbackModal modal)
  {
    await DeferAsync();

    // Get the rework from it's ID.
    HuisRework? rework = await GetReworkAsync(((SocketModal)Context.Interaction).Data.CustomId.Substring(12) /* pp_feedback_<rework ID> */);
    if (rework is null)
      return;

#if DEVELOPMENT || CUTTING_EDGE
    // In development or cutting edge mode, always send the feedback into the same channel.
    ISocketMessageChannel channel = Context.Channel;
#else
    // Get the PP Discord feedback channel. (WIP, other temp channel for now)
    ISocketMessageChannel channel = Context.Client.GetGuild(1166126757141827775).GetTextChannel(1264243048821293105);
#endif

    // Respond to the user and send the feedback in the feedback channel.
    await FollowupAsync(embed: Embeds.Success("Your feedback was submitted."));
    await channel.SendMessageAsync(embed: Embeds.Feedback(Context.User, rework, modal.FeebackText));
  }
}

public class FeedbackModal : IModal
{
  public string Title => "PP Feedback Form";

  /// <summary>
  /// The text of the feedback.
  /// </summary>
  [InputLabel("Feedback")]
  [ModalTextInput("text", TextInputStyle.Paragraph, "Type your feedback here... NOTE: The feedback is NOT anonymous, and publically visible.", 200)]
  public required string FeebackText { get; init; }
}
