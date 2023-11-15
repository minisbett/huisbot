using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot;

/// <summary>
/// A wrapper around a variable to cache it for a certain time.
/// </summary>
internal class Cached<T>
{
  private T _value = default!;
  /// <summary>
  /// The currently cached value.
  /// </summary>
  public T Value
  {
    get => _value;
    set
    {
      _lastRefresh = DateTime.UtcNow;
      _value = value;
    }
  }

  /// <summary>
  /// Bool whether the cached value is expired.
  /// </summary>
  public bool IsExpired => DateTime.UtcNow - _lastRefresh > _expirationSpan;

  /// <summary>
  /// The span after which the value expires.
  /// </summary>
  private TimeSpan _expirationSpan;

  /// <summary>
  /// The last refresh time of the value.
  /// </summary>
  private DateTime _lastRefresh = DateTime.MinValue;

  /// <summary>
  /// Creates a new cached value with the specified expiration span.
  /// The object is automatically expired by default until the value is updated.
  /// </summary>
  /// <param name="expirationSpan">The expiration span.</param>
  public Cached(TimeSpan expirationSpan)
  {
    _expirationSpan = expirationSpan;
  }
}
