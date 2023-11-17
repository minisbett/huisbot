using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents a queue entry on the Huis API.
/// </summary>
public class HuisQueueEntry
{
  /// <summary>
  /// The osu! ID of the queued user.
  /// </summary>
  [JsonProperty("user_id")]
  public int UserId { get; private set; }

  /// <summary>
  /// The ID of the rework of the queued entry.
  ///
  [JsonProperty("rework")]
  public int ReworkId { get; private set; }
}
