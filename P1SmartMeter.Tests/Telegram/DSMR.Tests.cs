using System;
using System.Text;
using Xunit;
using FluentAssertions;

using P1SmartMeter.Telegram.DSMR;

namespace P1SmartMeter.Tests.Telegram
{
    public class DSMRTests
    {
        [Fact]
        public void ParsesBasicTelegram1()
        {
            var t = new DSMRTelegram(MsgToString(message_1));
            t.Should().NotBeNull();
            t.TariffIndicator.Should().Be(1);
            t.TextMessage.Should().BeEmpty();
            t.ActualPowerUse.Should().Be(0);
            t.ActualPowerReturn.Should().Be(0.662);
            t.Timestamp.Should().Be(new DateTime(2021, 03, 07, 12, 37, 21));
            t.PowerUsedL1.Should().Be(0);
            t.PowerUsedL2.Should().Be(0.213);
            t.PowerUsedL3.Should().Be(0.076);
            t.PowerReturnedL1.Should().Be(0.9);
            t.PowerReturnedL2.Should().Be(0);
            t.PowerReturnedL3.Should().Be(0);
            t.VoltageL1.Should().Be(233.7);
            t.VoltageL2.Should().Be(233.6);
            t.VoltageL3.Should().Be(230.9);

            t.CurrentL1.Should().Be(3);
            t.CurrentL2.Should().Be(1);
            t.CurrentL3.Should().Be(0);
            t.Electricity1FromGrid.Should().Be(2236.186);
            t.Electricity1ToGrid.Should().Be(781.784);
            t.Electricity2FromGrid.Should().Be(1755.06);
            t.Electricity2ToGrid.Should().Be(1871.581);
            t.crc16.Should().Be("E1FA");
        }

        [Fact]
        public void ParsesBasicTelegram3()
        {
            var t = new DSMRTelegram(MsgToString(message_3));
            t.Should().NotBeNull();
            t.TariffIndicator.Should().Be(1);
            t.TextMessage.Should().BeEmpty();
            t.ActualPowerUse.Should().Be(0.335);
            t.ActualPowerReturn.Should().Be(0);
            t.Timestamp.Should().Be(new DateTime(2017, 11, 05, 20, 13, 24));
            t.PowerUsedL1.Should().Be(0.335);
            t.PowerUsedL2.Should().BeNull();
            t.PowerUsedL3.Should().BeNull();
            t.PowerReturnedL1.Should().Be(0);
            t.PowerReturnedL2.Should().BeNull();
            t.PowerReturnedL3.Should().BeNull();
            t.VoltageL1.Should().Be(229.0);
            t.VoltageL2.Should().BeNull();
            t.VoltageL3.Should().BeNull();
            t.CurrentL1.Should().Be(1);
            t.CurrentL2.Should().BeNull();
            t.CurrentL3.Should().BeNull();
            t.Electricity1FromGrid.Should().Be(51.775);
            t.Electricity1ToGrid.Should().Be(24.413);
            t.Electricity2FromGrid.Should().Be(0);
            t.Electricity2ToGrid.Should().Be(0);
            t.crc16.Should().Be("8F46");
        }

        private static string MsgToString(string[] message)
        {
            var str = new StringBuilder();
            foreach (string line in message)
            {
                str.Append(line + '\r' + '\n');
            }
            return str.ToString();
        }

        private static string[] message_1 = {
            @"/ISK5\2M550T-1013",
            @"",
            @"1-3:0.2.8(50)",
            @"0-0:1.0.0(210307123721W)",
            @"0-0:96.1.1(4530303534303037343736393431373139)",
            @"1-0:1.8.1(002236.186*kWh)",
            @"1-0:1.8.2(001755.060*kWh)",
            @"1-0:2.8.1(000781.784*kWh)",
            @"1-0:2.8.2(001871.581*kWh)",
            @"0-0:96.14.0(0001)",
            @"1-0:1.7.0(00.000*kW)",
            @"1-0:2.7.0(00.662*kW)",
            @"0-0:96.7.21(00004)",
            @"0-0:96.7.9(00004)",
            @"1-0:99.97.0(2)(0-0:96.7.19)(190911154933S)(0000000336*s)(201017081600S)(0000000861*s)",
            @"1-0:32.32.0(00003)",
            @"1-0:52.32.0(00001)",
            @"1-0:72.32.0(00002)",
            @"1-0:32.36.0(00001)",
            @"1-0:52.36.0(00001)",
            @"1-0:72.36.0(00001)",
            @"0-0:96.13.0()",
            @"1-0:32.7.0(233.7*V)",
            @"1-0:52.7.0(233.6*V)",
            @"1-0:72.7.0(230.9*V)",
            @"1-0:31.7.0(003*A)",
            @"1-0:51.7.0(001*A)",
            @"1-0:71.7.0(000*A)",
            @"1-0:21.7.0(00.000*kW)",
            @"1-0:41.7.0(00.213*kW)",
            @"1-0:61.7.0(00.076*kW)",
            @"1-0:22.7.0(00.900*kW)",
            @"1-0:42.7.0(00.000*kW)",
            @"1-0:62.7.0(00.000*kW)",
            @"!E1FA"
        };

        private static string[] message_2 = {
            @"/ISK5\2M550T-1013",
            @"",
            @"1-3:0.2.8(50)",
            @"0-0:1.0.0(210307123722W)",
            @"0-0:96.1.1(4530303534303037343736393431373139)",
            @"1-0:1.8.1(002236.186*kWh)",
            @"1-0:1.8.2(001755.060*kWh)",
            @"1-0:2.8.1(000781.784*kWh)",
            @"1-0:2.8.2(001871.581*kWh)",
            @"0-0:96.14.0(0001)",
            @"1-0:1.7.0(00.000*kW)",
            @"1-0:2.7.0(00.620*kW)",
            @"0-0:96.7.21(00004)",
            @"0-0:96.7.9(00004)",
            @"1-0:99.97.0(2)(0-0:96.7.19)(190911154933S)(0000000336*s)(201017081600S)(0000000861*s)",
            @"1-0:32.32.0(00003)",
            @"1-0:52.32.0(00001)",
            @"1-0:72.32.0(00002)",
            @"1-0:32.36.0(00001)",
            @"1-0:52.36.0(00001)",
            @"1-0:72.36.0(00001)",
            @"0-0:96.13.0()",
            @"1-0:32.7.0(233.4*V)",
            @"1-0:52.7.0(233.6*V)",
            @"1-0:72.7.0(230.7*V)",
            @"1-0:31.7.0(003*A)",
            @"1-0:51.7.0(001*A)",
            @"1-0:71.7.0(000*A)",
            @"1-0:21.7.0(00.000*kW)",
            @"1-0:41.7.0(00.211*kW)",
            @"1-0:61.7.0(00.072*kW)",
            @"1-0:22.7.0(00.900*kW)",
            @"1-0:42.7.0(00.000*kW)",
            @"1-0:62.7.0(00.000*kW)",
            @"!148A"
        };

        private static string[] message_3 = {
            @"/Ene5\XS210 ESMR 5.0",
            @"",
            @"1-3:0.2.8(50)",
            @"0-0:1.0.0(171105201324W)",
            @"0-0:96.1.1(4530303437303030303037363330383137)",
            @"1-0:1.8.1(000051.775*kWh)",
            @"1-0:1.8.2(000000.000*kWh)",
            @"1-0:2.8.1(000024.413*kWh)",
            @"1-0:2.8.2(000000.000*kWh)",
            @"0-0:96.14.0(0001)",
            @"1-0:1.7.0(00.335*kW)",
            @"1-0:2.7.0(00.000*kW)",
            @"0-0:96.7.21(00003)",
            @"0-0:96.7.9(00001)",
            @"1-0:99.97.0(0)(0-0:96.7.19)",
            @"1-0:32.32.0(00002)",
            @"1-0:32.36.0(00000)",
            @"0-0:96.13.0()",
            @"1-0:32.7.0(229.0*V)",
            @"1-0:31.7.0(001*A)",
            @"1-0:21.7.0(00.335*kW)",
            @"1-0:22.7.0(00.000*kW)",
            @"0-1:24.1.0(003)",
            @"0-1:96.1.0(4730303538353330303031313633323137)",
            @"0-1:24.2.1(171105201000W)(00016.713*m3)",
            @"!8F46"
        };
    }
}
