using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMS.Library;
using EMS.Library.DateTimeProvider;

namespace EMS.Engine
{
    internal class Measurement
    {
        public DateTime Received { get; init; }
        public double L1 { get; init; }
        public double L2 { get; init; }
        public double L3 { get; init; }
        public double L { get; init; }

        public double CL1 { get; init; }
        public double CL2 { get; init; }
        public double CL3 { get; init; }

        public Measurement(double l1, double l2, double l3, double cl1, double cl2, double cl3)
        {
            Received = DateTimeProvider.Now;
            L1 = l1;
            L2 = l2;
            L3 = l3;
            L = l1 + l2 + l3;
            CL1 = cl1;
            CL2 = cl2;
            CL3 = cl3;
        }        
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

        //todo create proper return obj
        public (
                int nrOfDataPoints,
                double avgCurrentUsingL1, double avgCurrentUsingL2, double avgCurrentUsingL3,
                double avgCurrentChargingL1, double avgCurrentChargingL2, double avgCurrentChargingL3
            ) CalculateAverageUsage()
        {
            Measurement[] m;
            lock (_measurements)
            {
                m = _measurements.ToArray();
            }
            if (m.Length == 0) {
                return (
                    nrOfDataPoints: 0,
                    avgCurrentUsingL1: 0, avgCurrentUsingL2: 0, avgCurrentUsingL3: 0,
                    avgCurrentChargingL1:0, avgCurrentChargingL2:0, avgCurrentChargingL3:0); }

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
            
            return (
                nrOfDataPoints: m.Length,
                avgCurrentUsingL1: avgCurrentUsingL1, avgCurrentUsingL2: avgCurrentUsingL2, avgCurrentUsingL3: avgCurrentUsingL3,
                avgCurrentChargingL1: avgCurrentChargingL1, avgCurrentChargingL2: avgCurrentChargingL2, avgCurrentChargingL3: avgCurrentChargingL3
                );
        }

        public (int nrOfDataPoints, double averageUsage, double averageCharge) CalculateAggregatedAverageUsage()
        {
            var (nrOfDataPoints, avgCurrentUsingL1, avgCurrentUsingL2, avgCurrentUsingL3, avgCurrentChargingL1, avgCurrentChargingL2, avgCurrentChargingL3) = CalculateAverageUsage();

            var avgCurrent = Math.Round(avgCurrentUsingL1 + avgCurrentUsingL2 + avgCurrentUsingL3, 2);
            var averageCharge = Math.Round(avgCurrentChargingL1 + avgCurrentChargingL2 + avgCurrentChargingL3, 2);

            return (nrOfDataPoints: nrOfDataPoints, averageUsage: avgCurrent, averageCharge: averageCharge);
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
