namespace huisbot.Helpers;

/// <summary>
/// Represents an expiring access token that can be registered as a singleton to be used in scoped API services.
/// </summary>
public class ExpiringAccessToken
{
  /// <summary>
  /// The access token.
  /// </summary>
  public string? Token { get; private set; }

  /// <summary>
  /// The expiration date of the access token.
  /// </summary>
  public DateTimeOffset ExpiresAt { get; private set; } = DateTimeOffset.MinValue;

  /// <summary>
  /// Bool whether the acceess token is expired.
  /// </summary>
  public bool IsExpired => DateTime.UtcNow > ExpiresAt;

  /// <summary>
  /// Sets the <see cref="Token"/> and <see cref="ExpiresAt"/> properties to the specified values.
  /// </summary>
  /// <param name="token">The access token.</param>
  /// <param name="expiresAt">The expiration date.</param>
  public void Renew(string token, DateTimeOffset expiresAt)
  {
    Token = token;
    ExpiresAt = expiresAt;
  }

  public override string ToString()
  {
    return $"Bearer {Token}";
  }
}
