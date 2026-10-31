using Microsoft.EntityFrameworkCore;
using MyResearchDesk.Web.Models;

namespace MyResearchDesk.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Article> Articles => Set<Article>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).ValueGeneratedOnAdd();
            e.Property(a => a.Title).IsRequired().HasMaxLength(300);
            e.Property(a => a.Url).IsRequired().HasMaxLength(2048);
            e.Property(a => a.Topic).IsRequired().HasMaxLength(100);
            e.Property(a => a.Source).HasMaxLength(200);
            e.Property(a => a.Takeaway).IsRequired().HasMaxLength(4000);
            e.Property(a => a.Status).HasConversion<int>().IsRequired();
            e.Property(a => a.CreatedAt).IsRequired();
            e.Property(a => a.UpdatedAt).IsRequired();

            e.HasIndex(a => a.Status);
            e.HasIndex(a => a.Topic);
            e.HasIndex(a => a.CreatedAt);
        });
    }
}
