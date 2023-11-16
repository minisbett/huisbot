﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Models.Utility;

/// <summary>
/// Represents a link between an osu! user and a discord user.
/// </summary>
public class OsuDiscordLink
{
  /// <summary>
  /// The discord user ID.
  /// </summary>
  [Key]
  public ulong DiscordId { get; set; }

  /// <summary>
  /// The osu! user ID.
  /// </summary>
  public int OsuId { get; set; }

  public OsuDiscordLink(ulong discordId, int osuId)
  {
    DiscordId = discordId;
    OsuId = osuId;
  }
}