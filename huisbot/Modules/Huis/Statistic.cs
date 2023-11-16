﻿using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Modules.Autocompletes;
using huisbot.Services;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System.Drawing;
using System.Drawing.Imaging;
using Color = System.Drawing.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the statistic command, displaying graphs for the top-statistics of a rework.
/// </summary>
public class StatisticCommandModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly HuisApiService _huis;

  public StatisticCommandModule(HuisApiService huis)
  {
    _huis = huis;
  }

  [SlashCommand("statistic", "Displays the specific top-statistic in the specified rework.")]
  public async Task HandleAsync(
    [Summary("statistic", "The top-statistic to display.")] [Choice("Top Scores", "topscores")] [Choice("Top Players", "topplayers")]
    string statisticId,
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")] [Autocomplete(typeof(ReworkAutocompleteHandler))]
    string reworkId)
  {
    await DeferAsync();
    bool topscores = statisticId == "topscores"; // True = topscores, False = topplayers

    // Get all reworks and check whether the request was successful. If not, notify the user about an internal error.
    HuisRework[]? reworks = await _huis.GetReworksAsync();
    if (reworks is null)
    {
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the reworks from the Huis API."));
      return;
    }

    // Try to find the specified rework by the specified identifier. If it doesn't exist, notify the user.
    HuisRework? rework = reworks.FirstOrDefault(x => x.Id.ToString() == reworkId || x.Code == reworkId || x.Name == reworkId);
    if (rework is null)
    {
      await FollowupAsync(embed: Embeds.Error($"The specified rework (`{reworkId}`) could not be found."));
      return;
    }

    // Get the statistic from the Huis API and check whether the request was successful. If not, notify the user.
    HuisStatistic? statistic = await (topscores ? _huis.GetTopScoresStatisticAsync(rework.Id) : _huis.GetTopPlayerStatisticAsync(rework.Id));
    if (statistic is null || statistic.Old is null || statistic.New is null || statistic.Difference is null)
    {
      await FollowupAsync(embed: Embeds.InternalError($"Failed to get the specified statistic (`{statisticId}`) from the Huis API."));
      return;
    }

    // Configure the plot with a size of 1000x600, a legend in the upper center, the axis labels and an extra axis on the right for the difference.
    Plot liveLocal = new Plot(1000, 600);
    liveLocal.Title($"{(topscores ? "Top Scores" : "Top Players")} - Live vs {rework.Name} @ {DateTime.UtcNow.ToShortDateString()} " +
                    $"{DateTime.UtcNow.ToLongTimeString()} (commit {rework.Commit})");
    liveLocal.LeftAxis.Label("Performance Points");
    liveLocal.BottomAxis.Label($"No. # Top {(topscores ? "Score" : "Player")}");
    liveLocal.AddAxis(Edge.Right, 2, "Difference", Color.Black);
    liveLocal.Legend(true, Alignment.UpperCenter);

    // Add the scatter lines of the old and new values and the difference.
    double[] xs = Enumerable.Range(0, statistic.Old.Length).Select(x => (double)x).ToArray();
    ScatterPlot diff = liveLocal.AddScatterLines(xs, statistic.Difference, Color.LightGray, 1, LineStyle.Solid, "Difference");
    diff.YAxisIndex = liveLocal.RightAxis.AxisIndex; // Make sure the difference is plotted on the right axis.
    liveLocal.AddScatterLines(xs, statistic.Old, Color.Red, 2, LineStyle.Solid, "Live");
    liveLocal.AddScatterLines(xs, statistic.New, Color.Blue, 2, LineStyle.Solid, "Local");

    // Render the plot to a bitmap and send it.
    using MemoryStream ms1 = new MemoryStream();
    liveLocal.Render().Save(ms1, ImageFormat.Png);
    await FollowupWithFileAsync(ms1, "plot.png");
  }
}