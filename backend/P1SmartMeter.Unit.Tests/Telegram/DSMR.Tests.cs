using System;
using System.Text;

using P1SmartMeter.Telegram.DSMR;


namespace P1SmartMeter.TelegramTests
{
    [SuppressMessage("","S125")]
    public class DSMRTests
    {
        [Fact]
        public void ParsesBasicTelegram1()
        {
            var t = new DSMRTelegram(MsgToString(message_1));
            t.Should().NotBeNull();
            t.VersionInformation.Should().Be("50");
            t.EquipmentIdentifier.Should().Be("E0054007476941719");
            t.TariffIndicator.Should().Be(1);
            t.TextMessage.Should().BeEmpty();
            t.ActualPowerUse.Should().Be(0);
            t.ActualPowerReturn.Should().Be(0.662);
            t.PowerFailureEventLog.Count.Should().Be(2);
            t.PowerFailureEventLog[0].Duration.Should().Be(336);
            t.PowerFailureEventLog[0].Timestamp.Should().Be(new DateTime(2019, 9, 11, 15, 49, 33));

            t.PowerFailureEventLog[1].Duration.Should().Be(861); 
            t.PowerFailureEventLog[1].Timestamp.Should().Be(new DateTime(2020, 10, 17, 08, 16, 00));

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
            t.Crc16.Should().Be("E1FA");
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
            t.Crc16.Should().Be("8F46");
        }

        [Fact]
        public void ParsesExampleTelegramFromDSMR()
        {
            var t = new DSMRTelegram(MsgToString(telegram_4), true);
            t.Should().NotBeNull();
            t.Timestamp.Should().Be(new DateTime(2010, 12, 09, 11, 30, 20));
            t.Electricity1FromGrid.Should().Be(123456.789);
            t.Electricity2FromGrid.Should().Be(123456.789);
            t.Electricity1ToGrid.Should().Be(123456.789);
            t.Electricity2ToGrid.Should().Be(123456.789);            
            t.PowerFailureEventLog.Count.Should().Be(2);

            t.PowerFailureEventLog[0].Timestamp.Should().Be(new DateTime(2010, 12, 08, 15, 24, 15));    // requirements mention 15:20:15, but seems 15:24:15
            t.PowerFailureEventLog[0].Duration.Should().Be(240);
            t.PowerFailureEventLog[1].Timestamp.Should().Be(new DateTime(2010, 12, 08, 15, 10, 04));    // requirements mention 15:05:03, but seems 15:10:04
            t.PowerFailureEventLog[1].Duration.Should().Be(301);
            t.PowerSagsL1.Should().Be(2);
            t.PowerSagsL2.Should().Be(1); //(poly phase meters only)
            t.PowerSagsL3.Should().Be(0); //(poly phase meters only)
            t.PowerSwellsL1.Should().Be(0);
            t.PowerSwellsL2.Should().Be(3); //(poly phase meters only)
            t.PowerSwellsL3.Should().Be(0); //(poly phase meters only)
            t.TextMessage.Should().Be(@"0123456789:;<=>?0123456789:;<=>?0123456789:;<=>?0123456789:;<=>?0123456789:;<=>?");

            t.TariffIndicator.Should().Be(2);
            t.VoltageL1.Should().Be(220.1);
            t.VoltageL2.Should().Be(220.2);
            t.VoltageL3.Should().Be(220.3);
            t.CurrentL1.Should().Be(1);
            t.CurrentL2.Should().Be(2);
            t.CurrentL3.Should().Be(3);
            t.PowerUsedL1.Should().Be(01.111);
            t.PowerUsedL2.Should().Be(02.222);
            t.PowerUsedL3.Should().Be(03.333);
            t.PowerReturnedL1.Should().Be(04.444);
            t.PowerReturnedL2.Should().Be(05.555);
            t.PowerReturnedL3.Should().Be(06.666);
            t.MBusDevice1.Should().NotBeNull();
            t.MBusDevice1.DeviceType.Should().Be(MBusDevice.DeviceTypes.Gas);
            t.MBusDevice1.Identifier.Should().Be("2222ABCD123456789");
            t.MBusDevice1.Measurement.Should().Be(12785.123);
            t.MBusDevice1.UnitOfMeasurement.Should().Be("m3");
            t.MBusDevice2.Should().Be(MBusDevice.NotPresent);
            t.MBusDevice3.Should().Be(MBusDevice.NotPresent);
            t.MBusDevice4.Should().Be(MBusDevice.NotPresent);
            t.Crc16.Should().Be("EF2F");
        }

        // not yet supporting eMUCS
        // there are a lot of differences in fields. Need a special class instead of trying to fix it in DSMRTelegram
        // - gas is not temp compensated (different fields)
        // - identifier as a different field
        //[Fact]
        //public void ParsesExampleTelegramFromeMUCS()
        //{
        //    var t = new DSMRTelegram(MsgToString(telegram_4b), true);
        //    t.Should().NotBeNull();
        //    t.MBusDevice1.Should().NotBeNull();
        //    t.MBusDevice1.DeviceType.Should().Be(MBusDevice.DeviceTypes.Gas);
        //    t.MBusDevice2.Should().NotBeNull();
        //    t.MBusDevice2.DeviceType.Should().Be(MBusDevice.DeviceTypes.Water);
        //    t.MBusDevice3.Should().Be(MBusDevice.NotPresent);
        //    t.MBusDevice4.Should().Be(MBusDevice.NotPresent);
        //}

        [Fact]
        public void ParsesExampleTelegramFromInternet1()
        {
            var t = new DSMRTelegram(MsgToString(telegram_5), true);
            t.Should().NotBeNull();
            t.MBusDevice1.Should().NotBeNull();
        }

        //[Fact]
        //public void ParsesExampleTelegramFromFluvius_1phase()
        //{
        //    var t = new DSMRTelegram(MsgToString(telegram_6_fluvius), true);
        //    t.Should().NotBeNull();
        //    t.MBusDevice1.Should().NotBeNull();
        //}

        [Fact]
        public void ParsesExampleTelegramFromInternet2()
        {
            var t = new DSMRTelegram(MsgToString(telegram_7), true);
            t.Should().NotBeNull();
            t.VersionInformation.Should().Be("42");

            t.MBusDevice1.Should().NotBeNull();
            t.MBusDevice1.DeviceType.Should().Be(MBusDevice.DeviceTypes.Gas);
            t.MBusDevice1.Identifier.Should().Be("G0025003346378516");
            t.MBusDevice2.Should().Be(MBusDevice.NotPresent);
            t.MBusDevice3.Should().Be(MBusDevice.NotPresent);
            t.MBusDevice4.Should().Be(MBusDevice.NotPresent);
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

        /*private static string[] message_2 = {
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
        };*/

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

        // This example telegram is taken from "Dutch Smart Meter Requirements v5.0.2 Final P1.docx" page 25
        // P1 telegram that is in accordance to IEC 62056-21 Mode D.
        private static string[] telegram_4 = {
            @"/ISk5\2MT382-1000",
            @"1-3:0.2.8(50)",
            @"0-0:1.0.0(101209113020W)",
            @"0-0:96.1.1(4B384547303034303436333935353037)",
            @"1-0:1.8.1(123456.789*kWh)",
            @"1-0:1.8.2(123456.789*kWh)",
            @"1-0:2.8.1(123456.789*kWh)",
            @"1-0:2.8.2(123456.789*kWh)",
            @"0-0:96.14.0(0002)",
            @"1-0:1.7.0(01.193*kW)",
            @"1-0:2.7.0(00.000*kW)",
            @"0-0:96.7.21(00004)",
            @"0-0:96.7.9(00002)",
            @"1-0:99.97.0(2)(0-0:96.7.19)(101208152415W)(0000000240*s)(101208151004W)(0000000301*s)",
            @"1-0:32.32.0(00002)",
            @"1-0:52.32.0(00001)",
            @"1-0:72.32.0(00000)",
            @"1-0:32.36.0(00000)",
            @"1-0:52.36.0(00003)",
            @"1-0:72.36.0(00000)",
            @"0-0:96.13.0(303132333435363738393A3B3C3D3E3F303132333435363738393A3B3C3D3E3F303132333435363738393A3B3C3D3E3F303132333435363738393A3B3C3D3E3F303132333435363738393A3B3C3D3E3F)",
            @"1-0:32.7.0(220.1*V)",
            @"1-0:52.7.0(220.2*V)",
            @"1-0:72.7.0(220.3*V)",
            @"1-0:31.7.0(001*A)",
            @"1-0:51.7.0(002*A)",
            @"1-0:71.7.0(003*A)",
            @"1-0:21.7.0(01.111*kW)",
            @"1-0:41.7.0(02.222*kW)",
            @"1-0:61.7.0(03.333*kW)",
            @"1-0:22.7.0(04.444*kW)",
            @"1-0:42.7.0(05.555*kW)",
            @"1-0:62.7.0(06.666*kW)",
            @"0-1:24.1.0(003)",
            @"0-1:96.1.0(3232323241424344313233343536373839)",
            @"0-1:24.2.1(101209112500W)(12785.123*m3)",
            @"!EF2F"
        };

        // This example telegram is taken from "e-MUCS_P1_Ed_1_7_1.pdf" page 16
        // 3-phase meter
        // P1 telegram example with a Gas meter on CH1 and a Water meter on CH2.
        /*private static string[] telegram_4b = {
            @"/FLU5\253769484_A",
            @"0-0:96.1.4(50217)",
            @"0-0:96.1.1(3153414733313031303231363035)",
            @"0-0:1.0.0(200512135409S)",
            @"1-0:1.8.1(000000.034*kWh)",
            @"1-0:1.8.2(000015.758*kWh)",
            @"1-0:2.8.1(000000.000*kWh)",
            @"1-0:2.8.2(000000.011*kWh)",
            @"1-0:1.4.0(02.351*kW)",
            @"1-0:1.6.0(200509134558S)(02.589*kW)",
            @"0-0:98.1.0(3)(1-0:1.6.0)(1-0:1.6.0)(200501000000S)(200423192538S)(03.695*kW)(200401000000S)(200305122139S)(05.980*kW)(200301000000S)(200210035421W)(04.318*kW)",
            @"1-0:2.7.0(00.000*kW)",
            @"1-0:21.7.0(00.000*kW)",
            @"1-0:41.7.0(00.000*kW)",
            @"1-0:61.7.0(00.000*kW)",
            @"1-0:22.7.0(00.000*kW)",
            @"1-0:42.7.0(00.000*kW)",
            @"1-0:62.7.0(00.000*kW)",
            @"1-0:32.7.0(234.7*V)",
            @"1-0:52.7.0(234.7*V)",
            @"1-0:72.7.0(234.7*V)",
            @"1-0:31.7.0(000.00*A)",
            @"1-0:51.7.0(000.00*A)",
            @"1-0:71.7.0(000.00*A)",
            @"0-0:96.3.10(1)",                              // Breaker state
            @"0-0:17.0.0(999.9*kW)",                        // Limiter threshold (999.9 = deactivated)
            @"1-0:31.4.0(999*A)",                           // Fuse supervision threshold (L1) (999 = deactivated)
            @"0-0:96.13.0()",                               // Text message (for future use (empty))
            @"0-1:24.1.0(003)",                             // gas
            @"0-1:96.1.1(37464C4F32313139303333373333)",    // equip ident
            @"0-1:24.4.0(1)",                               // valve state
            @"0-1:24.2.3(200512134558S)(00112.384*m3)",     // 'not temperature corrected' gas
            @"0-2:24.1.0(007)",                             // water
            @"0-2:96.1.1(3853414731323334353637383930)",    // equip ident
            @"0-2:24.2.1(200512134558S)(00872.234*m3)",     // last 5-minute water meter reading
            @"!XXX"
        };*/

        // telegram found on internet with some interresting items
        // 1) units of measurements directly after measurement (not sure if this is within spec)
        // 2) gas meter connected on sbus device 2
        // CRC isn't correct!
        private static string[] telegram_5 = {
            @"/ISK5\2M550E-1012",
            @"",
            @"1-3:0.2.8(50)",
            @"0-0:1.0.0(201022191257S)",
            @"0-0:96.1.1(453030343330300000000000000000)",
            @"1-0:1.8.1(002303.700*kWh)",
            @"1-0:1.8.2(001695.668*kWh)",
            @"1-0:2.8.1(000548.374*kWh)",
            @"1-0:2.8.2(001279.676*kWh)",
            @"0-0:96.14.0(0002)",
            @"1-0:1.7.0(00.452*kW)",
            @"1-0:2.7.0(00.000*kW)",
            @"0-0:96.7.21(00008)",
            @"0-0:96.7.9(00005)",
            @"1-0:99.97.0(3)(0-0:96.7.19)(190115011853W)(0000000389*s)(200217121327W)(0000000853*s)(200401114717S)(0000011165*s)",
            @"1-0:32.32.0(00006)",
            @"1-0:32.36.0(00001)",
            @"0-0:96.13.0()",
            @"1-0:32.7.0(232.4*V)",
            @"1-0:31.7.0(002*A)",
            @"1-0:21.7.0(00.449*kW)",
            @"1-0:22.7.0(00.000*kW)",
            @"0-1:24.1.0(003)",
            @"0-1:96.1.0()",
            @"0-1:24.2.1(700101010000W)(00000000)",
            @"0-2:96.1.0(4730303339303031383433313135303138)",
            @"0-2:24.2.1(201022191006S)(00847.336*m3)",
            @"!465C"
        };


        // fluvius example (eMUCs – P1 V1.6 page 14)
        // 1-phase meter with a Gas meter on CH1 and a Water meter on CH2)
        /*private static string[] telegram_6_fluvius = {
            @"/FLU5\253770234_A",
            @"",            
            @"0-0:96.1.4(50216)",
            @"0-0:96.1.1(3153414731313030303030323331)",
            @"0-0:1.0.0(200512145552S)",
            @"1-0:1.8.1(000000.915*kWh)",
            @"1-0:1.8.2(000001.955*kWh)",
            @"1-0:2.8.1(000000.000*kWh)",
            @"1-0:2.8.2(000000.030*kWh)",
            @"0-0:96.14.0(0001)",
            @"1-0:1.7.0(00.000*kW)",
            @"1-0:2.7.0(00.000*kW)",
            @"1-0:21.7.0(00.000*kW)",
            @"1-0:22.7.0(00.000*kW)",
            @"1-0:32.7.0(234.6*V)",
            @"1-0:31.7.0(000.00*A)",
            @"0-0:96.3.10(1)",
            @"0-0:17.0.0(999.9*kW)",
            @"1-0:31.4.0(999*A)",
            @"0-0:96.13.0()",
            @"0-1:24.1.0(003)",
            @"0-1:96.1.1(37464C4F32313139303333373333)",
            @"0-1:24.4.0(1)",
            @"0-1:24.2.3(200512134558S)(00112.384*m3)",
            @"0-2:24.1.0(007)",
            @"0-2:96.1.1(3853414731323334353637383930)",
            @"0-2:24.2.1(200512134558S)(00872.234*m3)",
            @"!XXXX"
        };*/

        // telegram found on internet 4.2
        private static string[] telegram_7 = {
            @"/KFM5KAIFA-METER",
            @"",
            @"1-3:0.2.8(42)",
            @"0-0:1.0.0(180605091333S)",
            @"0-0:96.1.1(4530303236303030303133343837363135)",
            @"1-0:1.8.1(001790.476*kWh)",
            @"1-0:1.8.2(002320.188*kWh)",
            @"1-0:2.8.1(000000.000*kWh)",
            @"1-0:2.8.2(000000.000*kWh)",
            @"0-0:96.14.0(0002)",
            @"1-0:1.7.0(00.258*kW)",
            @"1-0:2.7.0(00.000*kW)",
            @"0-0:96.7.21(00010)",
            @"0-0:96.7.9(00004)",
            @"1-0:99.97.0(1)(0-0:96.7.19)(000101000011W)(2147483647*s)",
            @"1-0:32.32.0(00000)",
            @"1-0:52.32.0(00000)",
            @"1-0:72.32.0(00000)",
            @"1-0:32.36.0(00000)",
            @"1-0:52.36.0(00000)",
            @"1-0:72.36.0(00000)",
            @"0-0:96.13.1()",
            @"0-0:96.13.0()",
            @"1-0:31.7.0(000*A)",
            @"1-0:51.7.0(000*A)",
            @"1-0:71.7.0(000*A)",
            @"1-0:21.7.0(00.125*kW)",
            @"1-0:22.7.0(00.000*kW)",
            @"1-0:41.7.0(00.124*kW)",
            @"1-0:42.7.0(00.000*kW)",
            @"1-0:61.7.0(00.009*kW)",
            @"1-0:62.7.0(00.000*kW)",
            @"0-1:24.1.0(003)",
            @"0-1:96.1.0(4730303235303033333436333738353136)",
            @"0-1:24.2.1(180605090000S)(05225.708*m3)",
            @"!F7F2"
        };
    }
}
