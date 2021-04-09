using System;
using EMS.Library;
using EMS.Library.Adapter.EVSE;
using Moq;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using static EMS.Compute;

namespace EMS.Test
{
    public class ComputeMaxSolar
    {
        [Fact]
        public void SolarHandlesMeasurementArrayNull()
        {
            var mock = new Mock<Compute>(ChargingMode.MaxSolar);
            mock.Object.Charging(null, new ChargingInfo(10f, 0f, 0f, 230f, 230f, 230f)).Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }
        [Fact]
        public void SolarHandlesLessThenMinimum()
        {
            var mock = new Mock<Compute>(ChargingMode.MaxSolar);
            
            for (int i = 0; i < Compute.MinimumDataPoints - 1; i++)
                mock.Object.AddMeasurement(new MeasurementBase(10, 0, 0, 0, 0, 0, 0, 0, 0));

            mock.Object.Charging(null, new ChargingInfo(10f, 0f, 0f, 230f, 230f, 230f)).Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }
        [Fact]
        public void MaxSolarNotEnoughProduction()
        {
            var mock = new Mock<Compute>(ChargingMode.MaxSolar);

            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 0, 0, 0.225, 0.165, 0.755, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0, 0.219, 0.216, 0.754, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0, 0.218, 0.208, 0.752, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0, 0.224, 0.208, 0.75, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0, 0.222, 0.214, 0.746, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0, 0.225, 0.264, 0.701, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0, 0.232, 0.272, 0.715, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(2, 1, 1, 0, 0.224, 0.218, 0.375, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(2, 1, 0, 0.341, 0.224, 0.17, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(4, 1, 1, 1.057, 0.216, 0.213, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(7, 1, 0, 1.758, 0.219, 0.162, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(10, 1, 1, 2.495, 0.222, 0.22, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(13, 1, 1, 2.984, 0.217, 0.214, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(13, 1, 1, 2.981, 0.215, 0.21, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(13, 1, 1, 2.984, 0.218, 0.213, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.937, 0.218, 0.29, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.939, 0.217, 0.246, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 2.945, 0.22, 0.171, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.942, 0.219, 0.191, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.94, 0.217, 0.222, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 2.938, 0.213, 0.091, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 2.928, 0.214, 0.102, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 2.923, 0.215, 0.096, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 2.972, 0.217, 0.098, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.929, 0.217, 0.218, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.975, 0.214, 0.275, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 2.976, 0.215, 0.189, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.975, 0.212, 0.212, 0, 0, 0));
        
            mock.Object.Charging(null,  new ChargingInfo(16f, 0f, 0f, 230f, 230f, 230f)).Should().Be((8.64f, 0, 0));  //4A?
            mock.Object.Charging(null,  new ChargingInfo(8.64f, 0f, 0f, 230f, 230f, 230f)).Should().Be((0f, 0, 0));
        }
        [Fact]
        public void MaxSolarTest2()
        {
            var mock = new Mock<Compute>(ChargingMode.MaxSolar);

            mock.Object.AddMeasurement(new MeasurementBase(11, 0, 0, 0, 0.146, 0.096, 2.822, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(11, 0, 0, 0, 0.146, 0.096, 2.822, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(11, 0, 0, 0, 0.146, 0.096, 2.822, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(11, 0, 0, 0, 0.146, 0.096, 2.822, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(11, 0, 0, 0, 0.146, 0.096, 2.822, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(11, 0, 0, 0, 0.146, 0.096, 2.822, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(11, 0, 0, 0, 0.146, 0.096, 2.822, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(11, 0, 0, 0, 0.146, 0.096, 2.822, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(11, 0, 0, 0, 0.146, 0.096, 2.822, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(11, 0, 0, 0, 0.146, 0.096, 2.822, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(11, 0, 0, 0, 0.146, 0.096, 2.822, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(11, 0, 0, 0, 0.146, 0.096, 2.822, 0, 0));
        
            
            mock.Object.Charging(null, new ChargingInfo(0f, 0f, 0f, 230f, 230f, 230f)).Should().Be((12.27f, 0, 0));
        }
        [Fact]
        public void MaxSolarTest3()
        {
            var mock = new Mock<Compute>(ChargingMode.MaxSolar);

            mock.Object.AddMeasurement(new MeasurementBase(3, 0, 0, 0, 0.146, 0.096, 0.750, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 0, 0, 0, 0.146, 0.096, 0.750, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 0, 0, 0, 0.146, 0.096, 0.750, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 0, 0, 0, 0.146, 0.096, 0.750, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 0, 0, 0, 0.146, 0.096, 0.750, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 0, 0, 0, 0.146, 0.096, 0.750, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 0, 0, 0, 0.146, 0.096, 0.750, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 0, 0, 0, 0.146, 0.096, 0.750, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 0, 0, 0, 0.146, 0.096, 0.750, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 0, 0, 0, 0.146, 0.096, 0.750, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 0, 0, 0, 0.146, 0.096, 0.750, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 0, 0, 0, 0.146, 0.096, 0.750, 0, 0));
        

            mock.Object.Charging(null, new ChargingInfo(0f, 0f, 0f, 230f, 230f, 230f)).Should().Be((0.0f, 0, 0), "we are not charging and return is not enough to charge");
            mock.Object.Charging(null, new ChargingInfo(6f, 0f, 0f, 230f, 230f, 230f)).Should().Be((9.26f, 0, 0), "we are charging at 6A, so we can increase charging");
            mock.Object.Charging(null, new ChargingInfo(8f, 0f, 0f, 230f, 230f, 230f)).Should().Be((11.26f, 0, 0), "we are charging at 8A, so we can increase charging");
            mock.Object.Charging(null, new ChargingInfo(10f, 0f, 0f, 230f, 230f, 230f)).Should().Be((13.26f, 0,0), "we are charging at 10A, so we can increase charging");
            mock.Object.Charging(null, new ChargingInfo(12f, 0f, 0f, 230f, 230f, 230f)).Should().Be((15.26f, 0, 0), "we are charging at 12A, so we can increase charging");
            mock.Object.Charging(null, new ChargingInfo(14f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16.0f, 0, 0), "we are charging at 14A, so we can increase charging to the max");
            mock.Object.Charging(null, new ChargingInfo(16f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16.0f, 0, 0), "we are charging at 16A, so we are already at the max");
        }
    }
    public class ComputeMaxCharge
    {

        [Fact]
        public void MaxChargeHandlesMeasurementArrayNull()
        {
            var mock = new Mock<Compute>(ChargingMode.MaxCharge);
            mock.Object.Charging(null,  new ChargingInfo(10f, 0f, 0f, 230f, 230f, 230f)).Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }
        [Fact]
        public void MaxChargeHandlesLessThenMinimum()
        {
            var mock = new Mock<Compute>(ChargingMode.MaxCharge);
            var lst = new List<MeasurementBase>();

            for (int i = 0; i < Compute.MinimumDataPoints - 1; i++)
                mock.Object.AddMeasurement(new MeasurementBase(10, 0, 0, 0, 0, 0, 0, 0, 0));                

            mock.Object.Charging(null, new ChargingInfo(10f, 0f, 0f, 230f, 230f, 230f)).Should().Be((-1, -1, -1), "-1 should be return when there are no or not enough samples");
        }

        [Fact]
        public void MaxChargeNotChargingYet()
        {
            var mock = new Mock<Compute>(ChargingMode.MaxCharge);

            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.221, 2.158, 0.778, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.221, 2.169, 0.778, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.225, 2.181, 0.76, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.226, 2.225, 0.76, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.237, 2.108, 0.761, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.24, 2.121, 0.761, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.223, 2.167, 0.781, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.226, 2.154, 0.764, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.227, 2.197, 0.765, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.229, 2.181, 0.765, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.223, 2.179, 0.765, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.226, 2.185, 0.767, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.23, 2.182, 0.768, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.225, 2.232, 0.767, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.223, 2.141, 0.77, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.219, 2.137, 0.772, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.223, 2.145, 0.771, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.227, 2.118, 0.774, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 8, 0, 0.23, 2.052, 0.774, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 8, 0, 0.226, 2.051, 0.774, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.228, 2.185, 0.773, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.224, 2.191, 0.772, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 8, 0, 0.22, 2.09, 0.764, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.223, 2.163, 0.734, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.221, 2.162, 0.772, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(1, 1, 9, 0, 0.229, 2.121, 0.229, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(2, 1, 9, 0.473, 0.222, 2.146, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 9, 0, 0.226, 2.177, 0.785, 0, 0));


            mock.Object.Charging(null, new ChargingInfo(0f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16f, 16f, 16f), "Because that is the maximum we can charge");
        }

        [Fact]
        public void MaxChargingTest1()
        {
            var mock = new Mock<Compute>(ChargingMode.MaxCharge);

            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 0, 0, 0.225, 0.165, 0.755, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0, 0.219, 0.216, 0.754, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0, 0.218, 0.208, 0.752, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0, 0.224, 0.208, 0.75, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0, 0.222, 0.214, 0.746, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0, 0.225, 0.264, 0.701, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(3, 1, 1, 0, 0.232, 0.272, 0.715, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(2, 1, 1, 0, 0.224, 0.218, 0.375, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(2, 1, 0, 0.341, 0.224, 0.17, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(4, 1, 1, 1.057, 0.216, 0.213, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(7, 1, 0, 1.758, 0.219, 0.162, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(10, 1, 1, 2.495, 0.222, 0.22, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(13, 1, 1, 2.984, 0.217, 0.214, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(13, 1, 1, 2.981, 0.215, 0.21, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(13, 1, 1, 2.984, 0.218, 0.213, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.937, 0.218, 0.29, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.939, 0.217, 0.246, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 2.945, 0.22, 0.171, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.942, 0.219, 0.191, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.94, 0.217, 0.222, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 2.938, 0.213, 0.091, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 2.928, 0.214, 0.102, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 2.923, 0.215, 0.096, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 2.972, 0.217, 0.098, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.929, 0.217, 0.218, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.975, 0.214, 0.275, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 0, 2.976, 0.215, 0.189, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(12, 1, 1, 2.975, 0.212, 0.212, 0, 0, 0));
        

            mock.Object.Charging(null, new ChargingInfo(16f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16f, 16f, 16f));
        }
        [Fact]
        public void MaxChargingTest2()
        {
            var mock = new Mock<Compute>(ChargingMode.MaxCharge);

            mock.Object.AddMeasurement(new MeasurementBase(17, 1, 5, 4.252, 0.218, 1.16, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.125, 0.218, 1.159, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.134, 0.236, 1.159, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(17, 1, 5, 4.109, 0.217, 1.161, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.268, 0.215, 1.16, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.134, 0.235, 1.163, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(17, 1, 5, 4.106, 0.226, 1.155, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.263, 0.227, 1.159, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.13, 0.221, 1.162, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(17, 1, 5, 4.093, 0.221, 1.16, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.143, 0.24, 1.164, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.161, 0.242, 1.161, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(17, 1, 5, 4.102, 0.224, 1.162, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.212, 0.225, 1.171, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.189, 0.238, 1.163, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(17, 1, 5, 4.079, 0.244, 1.16, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.144, 0.24, 1.165, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.137, 0.233, 1.159, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.075, 0.241, 1.164, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(17, 1, 5, 4.209, 0.24, 1.16, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(17, 1, 5, 4.143, 0.245, 1.161, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(17, 1, 5, 4.143, 0.247, 1.159, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.159, 0.228, 1.163, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.141, 0.234, 1.161, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(18, 1, 5, 4.145, 0.228, 1.163, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(17, 1, 5, 4.073, 0.234, 1.163, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(17, 1, 5, 4.138, 0.229, 1.164, 0, 0, 0));
            mock.Object.AddMeasurement(new MeasurementBase(17, 1, 5, 4.126, 0.223, 1.162, 0, 0, 0));
        

            mock.Object.Charging(null, new ChargingInfo(0f, 0f, 0f, 230f, 230f, 230f)).Should().Be((7.43f, 16f, 16f));
            mock.Object.Charging(null, new ChargingInfo(4f, 0f, 0f, 230f, 230f, 230f)).Should().Be((11.43f, 16f, 16f));
            mock.Object.Charging(null, new ChargingInfo(8f, 0f, 0f, 230f, 230f, 230f)).Should().Be((15.43f, 16f, 16f));
            mock.Object.Charging(null, new ChargingInfo(9f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16f, 16f, 16f));
            mock.Object.Charging(null, new ChargingInfo(10f, 0f, 0f, 230f, 230f, 230f)).Should().Be((16f, 16f, 16f));
        }


        
    }
}
