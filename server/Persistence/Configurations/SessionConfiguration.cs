using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripExpenseTracker.Api.Persistence.Entities;

namespace TripExpenseTracker.Api.Persistence.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(x => x.Id);

        // Hash of the session token. The raw token is never stored.
        builder.Property(x => x.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        // Logout removes every session for one user, so lookups by user must be indexed.
        builder.HasIndex(x => x.UserId);

        // Audit fields hold a user Id as text, up to 320 characters (the same limit as Email).
        builder.Property(x => x.CreatedBy)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(320);

        // Each session belongs to one user; deleting the user removes their sessions.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
