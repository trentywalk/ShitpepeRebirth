using Microsoft.EntityFrameworkCore;

namespace ShitpepeRebirth.Models
{
    internal class DBContext : DbContext
    {
        public DbSet<DiscordUser> DiscordUsers { get; set; } = null!;
        public DbSet<DiscordUserWallet> DiscordUserWallets { get; set; } = null!;

        public DBContext()
        {
            Database.EnsureCreatedAsync();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connection = new ForJsonConvertion().GetConnection(@"./config.json");
            optionsBuilder.UseSqlServer(connection);
        }
    }
}
