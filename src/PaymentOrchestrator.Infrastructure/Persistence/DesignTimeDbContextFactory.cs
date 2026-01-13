using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaymentOrchestrator.Infrastructure.Persistence;

internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PaymentOrchestratorDbContext>
{
    public PaymentOrchestratorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PaymentOrchestratorDbContext>();

        // Prefer environment-based connection string so secrets aren't checked in.
        // Try common environment keys used by ASP.NET Core configuration.
        // prefer SqlServer key used in appsettings.Development.sample.json
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
            ?? Environment.GetEnvironmentVariable("DefaultConnection");

        // Fallback to a sensible local DB for development if nothing provided.
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Server=(localdb)\\mssqllocaldb;Database=PaymentOrchestrator.Dev;Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        // Adjust the provider to match your project (SqlServer shown here).
        optionsBuilder.UseSqlServer(connectionString);

        return new PaymentOrchestratorDbContext(optionsBuilder.Options);
    }
}