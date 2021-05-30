using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMS.Library;
using EMS.Library.TestableDateTime;
using EMS.Library.Core;

namespace EMS.Engine
{
    public class AverageUsage
    {
        public AverageUsage() { }
        public AverageUsage(int nrOfDataPoints,
            double currentUsingL1, double currentUsingL2, double currentUsingL3,
            double currentChargingL1, double currentChargingL2, double currentChargingL3)
        {
            NrOfDataPoints = nrOfDataPoints;
            CurrentUsingL1 = currentUsingL1;
            CurrentUsingL2 = currentUsingL2;
            CurrentUsingL3 = currentUsingL3;
            CurrentChargingL1 = currentChargingL1;
            CurrentChargingL2 = currentChargingL2;
            CurrentChargingL3 = currentChargingL3;
        }

        public int NrOfDataPoints {get;set; }
        public double CurrentUsingL1 { get; set; }
        public double CurrentUsingL2 { get; set; }
        public double CurrentUsingL3 {get;set; }
        public double CurrentChargingL1 { get; set; }
        public double CurrentChargingL2 { get; set; }
        public double CurrentChargingL3 { get; set; }
    }

    public class Measurements
    {
        private readonly Queue<Measurement> _measurements = new(60);
        private int _bufferSeconds;
        public int BufferSeconds {
            get { return _bufferSeconds; }
            set
            {
                if (value < 10)
                    _bufferSeconds = 10;
                else
                    _bufferSeconds = (value > 1000) ? 1000 : value;
            }
        }

        public int ItemsInBuffer {  get { return _measurements.Count; } }

        public Measurements(int seconds)
        {
            _bufferSeconds = seconds;
        }       

        public void AddData(double? l1, double? l2, double? l3, double? chargerl1, double? chargerl2, double? chargerl3)
        {
            var s = new Measurement(l1.Value, l2.Value, l3.Value, chargerl1.Value, chargerl2.Value, chargerl3.Value);

            lock (_measurements)
            {
                _measurements.Enqueue(s);
                RemoveUnneededSamples();
            }
        }

        /***
         * Returns a copy the measurements
         */
        public IEnumerable<Measurement> GetMeasurements()
        {
            IEnumerable<Measurement> retval;
            lock (_measurements)
            {
                retval = _measurements.ToArray();
            }
            return retval;
        }

        public AverageUsage CalculateAverageUsage()
        {
            return CalculateAverageUsage(default);
        }

        public AverageUsage CalculateAverageUsage(DateTime? timeFrame)
        {
            Measurement[] m;
            lock (_measurements)
            {
                m = _measurements.ToArray();
            }
            return CalculateAverageUsage(m, timeFrame);
        }

        private static AverageUsage CalculateAverageUsage(IEnumerable<Measurement> m, DateTime? timeFrame)
        {
            if (timeFrame.HasValue)
                m = m.Where((x) => x.Received >= timeFrame);            

            if (!m.Any()) { return new AverageUsage(); }

            double avgCurrentUsingL1 = 0, avgCurrentUsingL2 = 0, avgCurrentUsingL3 = 0;
            double avgCurrentChargingL1 = 0, avgCurrentChargingL2 = 0, avgCurrentChargingL3 = 0;
            Parallel.Invoke(
                () => { avgCurrentUsingL1 = m.Average(x => x.L1); },
                () => { avgCurrentUsingL2 = m.Average(x => x.L2); },
                () => { avgCurrentUsingL3 = m.Average(x => x.L3); },
                () => { avgCurrentChargingL1 = m.Average(x => x.CL1); },
                () => { avgCurrentChargingL2 = m.Average(x => x.CL2); },
                () => { avgCurrentChargingL3 = m.Average(x => x.CL3); }
                );

            return new AverageUsage(
                 m.Count(),
                 avgCurrentUsingL1, avgCurrentUsingL2, avgCurrentUsingL3,
                 avgCurrentChargingL1, avgCurrentChargingL2, avgCurrentChargingL3
                );
        }

        public (int nrOfDataPoints, double averageUsage, double averageCharge) CalculateAggregatedAverageUsage()
        {
            return CalculateAggregatedAverageUsage(default);
        }

        public (int nrOfDataPoints, double averageUsage, double averageCharge) CalculateAggregatedAverageUsage(DateTime? timeFrame)
        {
            var avg = CalculateAverageUsage(timeFrame);

            var avgCurrent = Math.Round(avg.CurrentUsingL1 + avg.CurrentUsingL2 + avg.CurrentUsingL3, 2);
            var averageCharge = Math.Round(avg.CurrentChargingL1 + avg.CurrentChargingL2 + avg.CurrentChargingL3, 2);

            return (nrOfDataPoints: avg.NrOfDataPoints, averageUsage: avgCurrent, averageCharge: averageCharge);
        }

        private void RemoveUnneededSamples()
        {
            // remove stale data
            bool done = false;
            do
            {
                if (_measurements.TryPeek(out Measurement m))
                {
                    var t = DateTimeProvider.Now - m.Received;
                    if (t.TotalSeconds >= BufferSeconds)
                    {
                        _measurements.Dequeue();
                    }
                    else
                        done = true;
                }
                else
                    done = true;
            } while (!done);
        }
    }
}
