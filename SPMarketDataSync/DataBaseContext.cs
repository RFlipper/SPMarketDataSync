using Plumsail.SPMarketDataSync.Models;
using System.Data.Entity;

namespace Plumsail.SPMarketDataSync
{
    public class SPMarketDBContext : DbContext
    {
        public DbSet<App> Apps { get; set; }
        public DbSet<History> HistoricalData { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Types().Configure(entity => entity.ToTable("SPMarket_" + entity.ClrType.Name));
        }
    }
}
