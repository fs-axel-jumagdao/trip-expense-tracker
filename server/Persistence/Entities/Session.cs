namespace TripExpenseTracker.Api.Persistence.Entities;

public sealed class Session
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    // Hash of the random session token. The token itself is only held by the client cookie.
    public string TokenHash { get; set; } = string.Empty;

    // UTC. Set to login time plus 7 days.
    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    // The creating user's Id as text (up to 320 characters).
    public string CreatedBy { get; set; } = string.Empty;
    // Null until the session is first updated; sessions are not edited, only ended.
    public DateTimeOffset? UpdatedAt { get; set; }
    // The updating user's Id as text (up to 320 characters); null until the session is first updated.
    public string? UpdatedBy { get; set; }
}
