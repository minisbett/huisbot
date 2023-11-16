using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents the queue of all of Huismetbenen.
/// </summary>
public class HuisQueue
{
  /// <summary>
  /// The entries in the queue.
  /// </summary>
  [JsonProperty("queue")]
  public HuisQueueEntry[]? Entries { get; private set; }
}
