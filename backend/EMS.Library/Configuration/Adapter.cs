using System;
namespace EMS.Library.Configuration
{
    public enum AdapterType  { chargepoint, smartmeter, priceprovider };
    public class Adapter
    {
        public Guid Id { get; set; }
        public string Name  { get; set; }
        public AdapterType Type  { get; set; }
        public Driver Driver { get; set; }
    }

    public class Driver
    {
        public string Assembly { get; set; }
        public string Type { get; set; }
    }
}
