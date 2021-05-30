using System;
using EMS.Library.TestableDateTime;
namespace EMS.Library.Core
{
    public class Measurement
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
}
