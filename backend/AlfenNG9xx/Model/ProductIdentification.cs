using System;
namespace AlfenNG9xx.Model 
{
    public class ProductIdentification
    {
        public string Name {get; set;}
        public string Manufacterer {get; set; }
        public UInt16 TableVersion{get; set; }
        public string FirmwareVersion{get; set; }
        public string PlatformType{get; set; }                
        public string StationSerial{get; set; }
        public DateTime DateTimeUtc{get; set; }
        public DateTime DateTimeLocal{get; set; }
        public UInt64 Uptime{get; set; }
        public DateTime UpSinceUtc{get; set; }
        public int StationTimezone{get; set; }
    }
}
