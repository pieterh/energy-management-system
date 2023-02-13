using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace EMS.DataStore.InMemory
{
    public class DataProtectionKeyContext : DbContext, IDataProtectionKeyContext
    {
        public static string DBName = "DataProtection_EntityFrameworkCore";
        public DataProtectionKeyContext(DbContextOptions<DataProtectionKeyContext> options) : base(options) { }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}
