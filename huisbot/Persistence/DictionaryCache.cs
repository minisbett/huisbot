using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Persistence;

/// <summary>
/// A dictionary cache, caching non-expiring values by their unique ke.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">Thet ype of the value.</typeparam>
public class DictionaryCache<TKey, TValue> where TKey : ICacheableKey
{
    /// <summary>
    /// The cache dictionary, consisting of the <see cref="ICacheableKey.CacheUID"/> for the key and the value.
    /// </summary>
    private readonly Dictionary<string, TValue> _cache = new Dictionary<string, TValue>();

    /// <summary>
    /// Returns whether the cache contains an entry with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>Bool whether the cache contains an entry with the specified key.</returns>
    public bool Has(TKey key) => _cache.ContainsKey(key.CacheUID);

    /// <summary>
    /// Returns the currently cached value for the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The currently cached value for the specified key.</returns>
    public TValue Get(TKey key) => _cache[key.CacheUID];

    /// <summary>
    /// Adds a new entry to the cache.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void Add(TKey key, TValue value) => _cache.Add(key.CacheUID, value);
}
