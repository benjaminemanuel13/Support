using Microsoft.EntityFrameworkCore;
using Support.Common.Models;

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
    public DbSet<PlayWrightRequest> PlayWrightRequests { get; set; }
    public DbSet<WebTask> WebTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SupportArea>().ToTable("SupportAreas");
        modelBuilder.Entity<SpecificIssue>().ToTable("SpecificIssues");
        modelBuilder.Entity<Solution>().ToTable("Solutions");
    }
}
