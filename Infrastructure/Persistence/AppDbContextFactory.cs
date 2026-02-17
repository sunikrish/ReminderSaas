using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ReminderSaaS.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // Try to get connection string from environment variable first
         var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
                ?? "Server=localhost,1433;Database=ReminderDb;User Id=sa;Password=YourStrong!Pass123;TrustServerCertificate=True;";
            
        optionsBuilder.UseSqlServer(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}
