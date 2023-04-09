
using System;
using System.Text;

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
        public override StringBuilder ToPrintableString()
        {
            var retval = base.ToPrintableString();

            retval.AppendFormat("Table version              : {0}{1}", TableVersion, Environment.NewLine);            
            retval.AppendFormat("Platform type              : {0}{1}", PlatformType, Environment.NewLine);
            retval.AppendFormat("Station serial             : {0}{1}", StationSerial, Environment.NewLine);
            retval.AppendFormat("Date Local                 : {0}{1}", DateTimeLocal.ToString("O"), Environment.NewLine);
            retval.AppendFormat("Timezone                   : {0}{1}", StationTimezone, Environment.NewLine);
            return retval;
        }
    }
}
