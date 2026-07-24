using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClinicSite.Infrastructure.Data;

/// <summary>
/// Design-time factory so <c>dotnet ef</c> can build the context (for migrations) without spinning up
/// the whole API host. Only used by the EF tooling; the running app configures the context via DI.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ClinicSiteDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=ClinicSiteDb;Trusted_Connection=True;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
