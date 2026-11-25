using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripExpenseTracker.Api.Persistence.Entities;

namespace TripExpenseTracker.Api.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        // Stored trimmed and lowercased by the application, so the unique index is on the normalised email.
        builder.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        // Holds a password hash only, never the plain password.
        builder.Property(x => x.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        // Audit fields hold a user Id as text, up to 320 characters (the same limit as Email).
        builder.Property(x => x.CreatedBy)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(320);
    }
}
