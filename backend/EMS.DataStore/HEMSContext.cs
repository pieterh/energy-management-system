using System;
using Microsoft.EntityFrameworkCore;


namespace EMS.DataStore
{
	public class HEMSContext : DbContext
	{
        public string DbPath { get; }
        public DbSet<ChargingTransaction> ChargingTransactions { get; set; }

        public HEMSContext()
        {
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            DbPath = System.IO.Path.Join(folder, "hems.db");
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}

