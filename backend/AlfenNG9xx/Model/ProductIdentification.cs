
using System;
namespace AlfenNG9xx.Model 
{
    public record ProductIdentification : EMS.Library.ProductInformation
    {
        public UInt16 TableVersion {get; set; }
        public DateTime DateTimeUtc {get; set; }
        public DateTime DateTimeLocal {get; set; }
        public int StationTimezone {get; set; }
    }
}
