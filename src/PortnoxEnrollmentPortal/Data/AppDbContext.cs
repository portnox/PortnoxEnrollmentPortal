using Microsoft.EntityFrameworkCore;
using PortnoxEnrollmentPortal.Models;

namespace PortnoxEnrollmentPortal.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<EnrollmentRecord> Enrollments => Set<EnrollmentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnrollmentRecord>()
            .HasIndex(e => new { e.Upn, e.AdObjectGuid })
            .IsUnique(false);

        base.OnModelCreating(modelBuilder);
    }
}
