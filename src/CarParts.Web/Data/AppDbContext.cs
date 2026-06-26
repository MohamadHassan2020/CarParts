using CarParts.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CarParts.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Part> Parts => Set<Part>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Part>(e =>
        {
            e.HasIndex(p => p.PartNumber).IsUnique();
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            e.Property(p => p.RowVersion).IsConcurrencyToken();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Part>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.RowVersion = Guid.NewGuid();
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
