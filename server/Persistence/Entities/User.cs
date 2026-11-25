namespace TripExpenseTracker.Api.Persistence.Entities;

public sealed class User
{
    public Guid Id { get; set; }

    // Trimmed and lowercased by the application before insert or lookup. Unique in the database.
    public string Email { get; set; } = string.Empty;

    // Hash only (ASP.NET Core PasswordHasher<T>). The plain password is never stored.
    public string PasswordHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
    // The creating user's Id as text (up to 320 characters). For a user, this is their own Id.
    public string CreatedBy { get; set; } = string.Empty;
    // Null until the account is first updated; accounts are not edited in the MVP.
    public DateTimeOffset? UpdatedAt { get; set; }
    // The updating user's Id as text (up to 320 characters); null until the account is first updated.
    public string? UpdatedBy { get; set; }
}
