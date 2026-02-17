using Microsoft.EntityFrameworkCore;
using ReminderSaaS.Domain.Entities;

namespace ReminderSaaS.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Reminder> Reminders => Set<Reminder>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
                ?? "Server=localhost,1433;Database=ReminderDb;User Id=sa;Password=YourPassword123!;Encrypt=false;";
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reminder>().HasKey(r => r.Id);

        modelBuilder.Entity<Reminder>()
            .Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);
    }
}
