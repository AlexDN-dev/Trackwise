using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Trackwise.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets("4f7fd104-62ec-4163-9c56-38cf8f19b0fb").Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'DefaultConnection' introuvable (User Secrets ?).");

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options;

        return new AppDbContext(options);
    }
}