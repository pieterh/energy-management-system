using System;
using System.Collections.Generic;
using System.Linq;
using EMS.Library.Core;

namespace EMS.Library.Adapter.EVSE
{
    public record ChargeSessionInfoBase
    {
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public UInt32 ChargingTime { get; set; }
        public double EnergyDelivered { get; set; }
        public bool SessionEnded { get; set; }
        public Decimal Cost
        {
            get
            {
                var d = Costs.Sum((x) => { 
                    var energy = x.Energy >= 0.0m ? x.Energy / 1000.0m : 0.0m; 
                    var tariffUsage = x?.Tariff?.TariffUsage ?? 0.0m;
                    return tariffUsage * energy; 
                }) + RunningCost;
                return d;
            }
        }
        public Decimal RunningCost { get; set; }
        public required IList<Cost> Costs { get; init; }
    }
}
