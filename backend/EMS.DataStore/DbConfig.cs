using System;
namespace EMS.DataStore
{
    public record DbConfig
    {
        public enum DbType { sqlite };

        public DbType type { get; set; }
        public string name { get; set; } = default!;
    }
}

