using Microsoft.EntityFrameworkCore;

namespace Support.Data;

public class SupportContext : DbContext
{
    public SupportContext(DbContextOptions<SupportContext> options)
        : base(options)
    {
    }

    public DbSet<SupportArea> SupportAreas { get; set; }
    public DbSet<SpecificIssue> SpecificIssues { get; set; }
    public DbSet<Solution> Solutions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SupportArea>().ToTable("SupportAreas");
        modelBuilder.Entity<SpecificIssue>().ToTable("SpecificIssues");
        modelBuilder.Entity<Solution>().ToTable("Solutions");
    }
}
