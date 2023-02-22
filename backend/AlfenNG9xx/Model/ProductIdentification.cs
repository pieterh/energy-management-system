
using System;
namespace AlfenNG9xx.Model 
{
    /* Platform type
     * NG900 Single S-line
     * NG910 Single Pro-line
     * NG920 Eve Double Pro-line / Eve Double PG / Twin 4XL 
     */
    public record ProductIdentification : EMS.Library.ProductInformation
    {
        public UInt16 TableVersion {get; set; }        
        public DateTime DateTimeLocal {get; set; }
        public int StationTimezone {get; set; }
        public string PlatformType { get; set; } = default!;
    }
}
