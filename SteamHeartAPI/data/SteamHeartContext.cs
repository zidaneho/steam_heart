using Microsoft.EntityFrameworkCore;
using SteamHeartAPI.Models; // Ensure this matches your namespace for the Game class

namespace SteamHeartAPI.Data
{
    public class SteamHeartContext : DbContext
    {
        public SteamHeartContext(DbContextOptions<SteamHeartContext> options)
            : base(options)
        {
        }

        // This tells EF Core: "I want a table called 'Games' based on the 'Game' class"
        public DbSet<Game> GameTable { get; set; }
        public DbSet<Metric> MetricTable { get; set; }
        public DbSet<Review> ReviewTable { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()
                .HasIndex(g => g.AppId)
                .IsUnique();

            modelBuilder.Entity<Game>()
                .Property(g => g.Tags)
                .HasColumnType("jsonb");

            modelBuilder.Entity<Metric>()
                .HasIndex(m => new { m.GameId, m.Timestamp });

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.RecommendationId)
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.GameId, r.TimestampCreated });

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.Language);

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.VotedUp);

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.AuthorSteamId);
        }
    }
}