﻿namespace huisbot.Persistence.Caching;

/// <summary>
/// Makes a type eligible to be used as a key for the dictionary cache by providing a CacheUID property.
/// </summary>
public interface ICacheableKey
{
  /// <summary>
  /// A unique identifier for caching values in a <see cref="DictionaryCache{TKey, TValue}"/>, using this as the key.
  /// </summary>
  public string CacheUID { get; }
}
