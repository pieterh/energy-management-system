using System;
namespace EMS.Library.Configuration
{
    public enum AdapterType  { chargepoint, smartmeter, priceprovider, solar };
    public record AdapterConfiguration
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public AdapterType Type  { get; set; }
        public Driver Driver { get; set; } = default!;
    }

    public record Driver
    {
        public string Assembly { get; set; } = default!;
        public string Type { get; set; } = default!;
    }
}
