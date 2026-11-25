using Microsoft.EntityFrameworkCore;
using TripExpenseTracker.Api.Persistence.Configurations;
using TripExpenseTracker.Api.Persistence.Entities;

namespace TripExpenseTracker.Api.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Session> Sessions => Set<Session>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new SessionConfiguration());
    }
}
