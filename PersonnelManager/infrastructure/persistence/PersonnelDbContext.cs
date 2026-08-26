using Microsoft.EntityFrameworkCore;
using PersonnelManager.Domain;

namespace PersonnelManager.Infrastructure.Persistence;

/// <summary>
/// The EF Core context for the personnel database. Maps the <see cref="Personal"/> entity to a
/// PostgreSQL table; the enum is stored as text for readability.
/// </summary>
public sealed class PersonnelDbContext(DbContextOptions<PersonnelDbContext> options)
    : DbContext(options)
{
    public DbSet<Personal> Personnel => Set<Personal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var personal = modelBuilder.Entity<Personal>();

        personal.ToTable("personnel");
        personal.HasKey(p => p.Id);

        // Store the enum as its name ("Active", "OnLeave", ...) rather than an int.
        personal.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        personal.Property(p => p.Name).HasMaxLength(200);
        personal.Property(p => p.Surname).HasMaxLength(200);
        personal.Property(p => p.Address).HasMaxLength(500);
        personal.Property(p => p.Phone).HasMaxLength(50);
    }
}
