using System;
using Microsoft.EntityFrameworkCore;


namespace EMS.DataStore
{
	public class HEMSContext : DbContext
	{
        public string DbPath { get; }
// Disable nullable warning, since the entity framework will set the property ChargingTransactions
#pragma warning disable CS8618
        public DbSet<ChargingTransaction> ChargingTransactions { get; set; }

        public HEMSContext()
        {
#pragma warning restore CS8618
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            DbPath = System.IO.Path.Join(folder, "hems.db");
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}

