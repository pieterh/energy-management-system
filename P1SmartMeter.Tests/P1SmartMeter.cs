using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Moq;
using Moq.Protected;
using FluentAssertions;

using P1SmartMeter;

namespace P1SmartMeter.Tests
{
    public class MessageBufferTests
    {
        //private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const string tc1 = "01 - Complete message 1";
        private const string tc2 = "02 - Complete message 2";
        private const string tc3 = "03 - Complete message arriving in two chunks";
        private const string tc3b = "03b - Complete message arriving in multiple chunks";

        private const string tc4 = "04 - Last part message 1, followed by complete message";
        private const string tc5 = "05 - First part message 1, followed by complete message 2";

        private const string tc6 = "06 - Corrupt message 1, followed by complete message 2";




        private Dictionary<string, TestItem[]> dataSet = new Dictionary<string, TestItem[]>();
        public MessageBufferTests()
        {
            dataSet.Add(tc1, new TestItem[]
                { new TestItem() { data = MsgToString(message_1) , expectMessage=true, expectError=false } }
            );

            dataSet.Add(tc2, new TestItem[]
                { new TestItem() { data = MsgToString(message_2) , expectMessage=true, expectError=false } }
            );

            dataSet.Add(tc3, new TestItem[]
                {
                    new TestItem() { data = MsgToString(message_1).Substring(0, 10) , expectMessage=false, expectError=false } ,
                    new TestItem() { data = MsgToString(message_1).Substring(10) , expectMessage=true, expectError=false }
                }
            );

            dataSet.Add(tc3b, new TestItem[]
                {
                    new TestItem() { data = MsgToString(message_1).Substring(0, 10) , expectMessage=false, expectError=false } ,
                    new TestItem() { data = MsgToString(message_1).Substring(10, 10) , expectMessage=false, expectError=false } ,
                    new TestItem() { data = MsgToString(message_1).Substring(20, 10) , expectMessage=false, expectError=false } ,
                    new TestItem() { data = MsgToString(message_1).Substring(30, 10) , expectMessage=false, expectError=false } ,
                    new TestItem() { data = MsgToString(message_1).Substring(40) , expectMessage=true, expectError=false }
                }
            );

            dataSet.Add(tc4, new TestItem[]
                {
                    new TestItem() { data = MsgToString(message_1).Substring(MsgToString(message_1).Length - 25) , expectMessage = false, expectError = true },
                    new TestItem() { data = MsgToString(message_2) , expectMessage = true, expectError = false }
                }
            );

            dataSet.Add(tc5, new TestItem[]
                {
                                new TestItem() { data = MsgToString(message_1).Substring(0, 10) , expectMessage = false, expectError = false } ,
                                new TestItem() { data = MsgToString(message_2) , expectMessage = true, expectError = true } ,
                }
            );

            dataSet.Add(tc6, new TestItem[]
                {
                    new TestItem() { data = MsgToString(message_1).Substring(0, 10) , expectMessage = false, expectError = false } ,
                    new TestItem() { data = MsgToString(message_1).Substring(MsgToString(message_1).Length - 25) , expectMessage = false, expectError = true } ,
                    new TestItem() { data = MsgToString(message_2).Substring(0, 10) , expectMessage = false, expectError = false } ,
                    new TestItem() { data = MsgToString(message_2).Substring(10) , expectMessage = true, expectError = false }
                }
            );
        }

        [Theory]
        [InlineData(tc1)]
        [InlineData(tc2)]
        [InlineData(tc3)]
        [InlineData(tc3b)]
        [InlineData(tc4)]
        [InlineData(tc5)]
        [InlineData(tc6)]
        public void Add(string testCase)
        {
            var items = dataSet[testCase];
            var mock = new Mock<MessageBuffer>();
            var errorRaised = false;

            mock.Object.DataError += (sender, args) =>
            {
                errorRaised = true;
            };

            foreach (var item in items)
            {
                errorRaised = false;
                var hasMessage = mock.Object.Add(item.data);
                string msg;

                mock.Object.TryTake(out msg).Should().Be(item.expectMessage);
                errorRaised.Should().Be(item.expectError);
                Console.WriteLine(item.data);
            }
        }

        //[Fact]
        //public void SingleBufferOverflow()
        //{
        //    var mock = new Mock<MessageBuffer>();
        //    var errorRaised = false;

        //    mock.Object.DataError += (sender, args) =>
        //    {
        //        errorRaised = true;
        //    };
        //    for (int i = 0; i < MessageBuffer.MAX_MESSAGES_IN_BUFFER; i++)
        //    {
        //        mock.Object.Add(MsgToString(message_1));
        //    }
        //    errorRaised.Should().Be(false, "Since we are not overflowing yet");
        //    mock.Object.Add(MsgToString(message_1));
        //    errorRaised.Should().Be(true, "Since we are overflowing");
        //}

        //[Fact]
        //public void RemovesFirstOnBufferOverflow()
        //{
        //    var mock = new Mock<MessageBuffer>();
        //    var errorRaised = false;

        //    mock.Object.DataError += (sender, args) =>
        //    {
        //        errorRaised = true;
        //    };
        //    mock.Object.Add(MsgToString(message_1));
        //    for (int i = 0; i < MessageBuffer.MAX_MESSAGES_IN_BUFFER - 1; i++)
        //    {
        //        mock.Object.Add(MsgToString(message_2));
        //    }
        //    errorRaised.Should().Be(false, "Because the buffer is not yet full");
        //    mock.Object.Add(MsgToString(message_2));
        //    errorRaised.Should().Be(true, "Because the buffer is now full");
        //    var msg = string.Empty;
        //    mock.Object.MessageQueue.TryTake(out msg, 100).Should().Be(true);
        //    msg.Should().BeEquivalentTo(MsgToString(message_2), "Since the first message is dropped");
        //}

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

        public class TestItem
        {
            public string data { get; set; }
            public bool expectMessage { get; set; }
            public bool expectError { get; set; }
        }
    }
}