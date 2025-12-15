using Microsoft.EntityFrameworkCore;
using SteamAlertsAPI.Models; // Ensure this matches your namespace for the Game class

namespace SteamAlertsAPI.Data
{
    public class SteamAlertsContext : DbContext
    {
        public SteamAlertsContext(DbContextOptions<SteamAlertsContext> options)
            : base(options)
        {
        }

        // This tells EF Core: "I want a table called 'Games' based on the 'Game' class"
        public DbSet<Game> Games { get; set; }
    }
}