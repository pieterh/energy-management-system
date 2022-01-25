using System;
using System.Collections.Generic;
using System.Linq;
using EMS.Library.Core;

namespace EMS.Library.Adapter.EVSE
{
    public record ChargeSessionInfoBase
    {
        public Nullable<DateTime> Start { get; set; }
        public Nullable<DateTime> End { get; set; }
        public UInt32 ChargingTime { get; set; }
        public double EnergyDelivered { get; set; }
        public bool SessionEnded { get; set; }
        public Decimal Cost
        {
            get
            {
                var d = Costs.Sum((x) => { var energy = x.Energy >= 0.0m ? x.Energy / 1000.0m : 0.0m; return x.Tariff.TariffUsage * energy; })
                       + RunningCost;
                return d;
            }
        }
        public Decimal RunningCost { get; set; }
        public IList<Cost> Costs { get; set; }
    }
}
