
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EMS.DataStore;

[SuppressMessage("SonarLint", "S101", Justification = "Ignored intentionally")]
public class HEMSContext : DbContext
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    // nog te doen: use more elegant way of passing the DbPath
    // https://www.codeproject.com/Articles/5281767/Scalable-Scenario-to-Configuring-Entity-Framework
    public static string DbPath { get; set; } = default!;
    public DbSet<ChargingTransaction> ChargingTransactions { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;

    public HEMSContext()
    {
        if (string.IsNullOrWhiteSpace(DbPath))
        {
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            DbPath = System.IO.Path.Join(folder, "hems.db");
            Logger.Warn("No name for database file given. Defaults to database file {DbPath}", DbPath);
        }
        else
            Logger.Debug("Using database file {DbPath}", DbPath);
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={DbPath}");

    internal static DateTime cvt(DateTime dt)
    {
        if (dt.Kind == DateTimeKind.Unspecified)
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        else
            dt = dt.ToUniversalTime();
        return dt;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ArgumentNullException.ThrowIfNull(modelBuilder);

        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => cvt(v), //v.ToUniversalTime(),
                v => cvt(v)
            );

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
            );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsKeyless)
            {
                continue;
            }

            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }
    }
}
