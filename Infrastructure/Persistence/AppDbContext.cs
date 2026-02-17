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
    public DbSet<Schedule> Schedules => Set<Schedule>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
                ?? "Server=localhost,1433;Database=ReminderDb;User Id=sa;Password=YourStrong!Pass123;TrustServerCertificate=True;";
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

        // Configure Schedule entity
        modelBuilder.Entity<Schedule>().HasKey(s => s.Id);

        modelBuilder.Entity<Schedule>()
            .Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<Schedule>()
            .Property(s => s.Description)
            .HasMaxLength(1000);

        modelBuilder.Entity<Schedule>()
            .Property(s => s.Location)
            .HasMaxLength(255);

        // Store enum as string
        modelBuilder.Entity<Schedule>()
            .Property(s => s.Category)
            .HasConversion<string>();

        // Create index on Date for efficient queries
        modelBuilder.Entity<Schedule>()
            .HasIndex(s => s.Date);

        // Create composite index for month/year queries
        modelBuilder.Entity<Schedule>()
            .HasIndex(s => new { s.Date, s.Category });
    }
}
