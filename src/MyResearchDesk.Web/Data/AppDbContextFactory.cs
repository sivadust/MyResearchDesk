using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyResearchDesk.Web.Data;

// Lets `dotnet ef` run without executing Program.cs (which migrates at startup).
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=researchdesk;Username=researchdesk;Password=researchdesk")
            .Options;
        return new AppDbContext(options);
    }
}
