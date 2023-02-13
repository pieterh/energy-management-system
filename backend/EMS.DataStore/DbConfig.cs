using System;
namespace EMS.DataStore
{
    public record DbConfig
    {
        public enum DbTypeEnum { sqlite };

        public DbTypeEnum DbType { get; set; }
        public string dbname { get; set; } = default!;
    }
}

