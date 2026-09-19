using Microsoft.EntityFrameworkCore;
using System.Configuration;
using PurePostBot.models;

namespace PurePostBot
{
    public class AppDbContext : DbContext
    {
        // Configuration
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = ConfigurationManager
                .ConnectionStrings["DefaultConnection"]
                .ConnectionString ?? throw new InvalidOperationException("Connection string is null");

            optionsBuilder.UseSqlite(connectionString);
        }

        public DbSet<UserClient> Users { get; set; }
    }
}
