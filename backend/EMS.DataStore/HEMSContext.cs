using System;
using Microsoft.EntityFrameworkCore;


namespace EMS.DataStore
{
	public class HEMSContext : DbContext // NOSONAR
	{
        // nog te doen: use more elegant way of passing the DbPath
        // https://www.codeproject.com/Articles/5281767/Scalable-Scenario-to-Configuring-Entity-Framework
        public static string DbPath { get; set; } = default!;
        public DbSet<ChargingTransaction> ChargingTransactions { get; set; } = default!;

        public HEMSContext()
        {
            if (string.IsNullOrWhiteSpace(DbPath))
            {
                var folder = AppDomain.CurrentDomain.BaseDirectory;
                DbPath = System.IO.Path.Join(folder, "hems.db");
            }
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}

