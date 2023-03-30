using System;
using System.Text;

using Xunit;
using Moq;
using FluentAssertions;

using P1SmartMeter;

namespace P1SmartMeter.MessageBufferTests
{
    public class MessageBufferTests
    {
        private const string tc01 = "01 - some random data";
        private const string tc02 = "02 - Complete message 1";
        private const string tc03 = "03 - Complete message 2";
        private const string tc04 = "04 - Complete message arriving in two chunks";
        private const string tc05 = "05 - Complete message arriving in multiple chunks";

        private const string tc06 = "06 - Last part message 1, followed by complete message 2";
        private const string tc07 = "07 - First part message 1, followed by complete message 2";
        private const string tc08 = "08 - First part message 1, followed by complete message 2 in mulitple chunks";
        private const string tc09 = "09 - First part message 1, followed by complete message 2 in mulitple chunks";

        private const string tc10 = "10 - Corrupt message 1, followed by complete message 2";
        private const string tc11 = "11 - Partial message 1 that includes end-marker and crc is in second chunk";
        private const string tc12 = "12 - Partial message 1 that includes end-marker, followed by the rest in multiple chunks";
        private const string tc13 = "13 - Overflowing buffer with random data";
        private const string tc14 = "14 - Overflowing buffer with new complete message";

        private Dictionary<string, TestItem[]> dataSet = new Dictionary<string, TestItem[]>();
        public MessageBufferTests()
        {
            dataSet.Add(tc01, new TestItem[]
                {
                    new TestItem() { Data = "A" , ExpectMessage=false, ExpectError=true }
                }
            );

            dataSet.Add(tc02, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_1) , ExpectMessage=true, ExpectError=false }
                }
            );

            dataSet.Add(tc03, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_2) , ExpectMessage=true, ExpectError=false }
                }
            );

            dataSet.Add(tc04, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_1).Substring(0, 10) , ExpectMessage=false, ExpectError=false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(10) , ExpectMessage=true, ExpectError=false }
                }
            );

            dataSet.Add(tc05, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_1).Substring(0, 10) , ExpectMessage=false, ExpectError=false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(10, 10) , ExpectMessage=false, ExpectError=false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(20, 10) , ExpectMessage=false, ExpectError=false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(30, 10) , ExpectMessage=false, ExpectError=false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(40) , ExpectMessage=true, ExpectError=false }
                }
            );

            dataSet.Add(tc06, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_1).Substring(MsgToString(message_1).Length - 25) , ExpectMessage = false, ExpectError = true },
                    new TestItem() { Data = MsgToString(message_2) , ExpectMessage = true, ExpectError = false }
                }
            );

            dataSet.Add(tc07, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_1).Substring(0, 10) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_2) , ExpectMessage = true, ExpectError = true } ,
                }
            );

            dataSet.Add(tc08, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_1).Substring(0, 30) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_2).Substring(0, 10) , ExpectMessage=false, ExpectError=true } ,
                    new TestItem() { Data = MsgToString(message_2).Substring(10, 10) , ExpectMessage=false, ExpectError=false } ,
                    new TestItem() { Data = MsgToString(message_2).Substring(20), ExpectMessage=true, ExpectError=false } ,
                }
            );

            dataSet.Add(tc09, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_1).Substring(0, 10) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_2).Substring(0, 10) , ExpectMessage=false, ExpectError=true } ,
                    new TestItem() { Data = MsgToString(message_2).Substring(10, 10) , ExpectMessage=false, ExpectError=false } ,
                    new TestItem() { Data = MsgToString(message_2).Substring(20), ExpectMessage=true, ExpectError=false } ,
                }
            );

            dataSet.Add(tc10, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_1).Substring(0, 10) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(MsgToString(message_1).Length - 25) , ExpectMessage = false, ExpectError = true } ,
                    new TestItem() { Data = MsgToString(message_2).Substring(0, 10) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_2).Substring(10) , ExpectMessage = true, ExpectError = false }
                }
            );
            dataSet.Add(tc11, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_1).Substring(0, MsgToString(message_1).Length - (4+2)) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(MsgToString(message_1).Length - (4+2)) , ExpectMessage = true, ExpectError = false } ,
                }
            );
            dataSet.Add(tc12, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_1).Substring(0, MsgToString(message_1).Length - (4+2)) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(MsgToString(message_1).Length - (6), 1) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(MsgToString(message_1).Length - (5), 1) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(MsgToString(message_1).Length - (4), 1) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(MsgToString(message_1).Length - (3), 1) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(MsgToString(message_1).Length - (2), 1) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(MsgToString(message_1).Length - (1), 1) , ExpectMessage = true, ExpectError = false }
                }
            );
            dataSet.Add(tc13, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_1).Substring(0, 10) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(10, 5) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = new string('X', MessageBuffer.BufferCapacity / 3) , ExpectMessage = false, ExpectError = false },
                    new TestItem() { Data = new string('Y', MessageBuffer.BufferCapacity / 3) , ExpectMessage = false, ExpectError = false },
                    new TestItem() { Data = new string('Z', MessageBuffer.BufferCapacity / 3) , ExpectMessage = false, ExpectError = true },
                    new TestItem() { Data = MsgToString(message_1) , ExpectMessage = true, ExpectError = false }
                }
            );
            dataSet.Add(tc14, new TestItem[]
                {
                    new TestItem() { Data = MsgToString(message_1).Substring(0, 10) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = MsgToString(message_1).Substring(10, 5) , ExpectMessage = false, ExpectError = false } ,
                    new TestItem() { Data = new string('A', MessageBuffer.BufferCapacity - 40) , ExpectMessage = false, ExpectError = false },
                    new TestItem() { Data = string.Concat("/ISK5\\2M550T-10131-0:62.7.0(00.000*kW)!148A\r\n", MsgToString(message_1), MsgToString(message_1)) , ExpectMessage = true, ExpectError = true }
                }
            );
        }

        [Theory]
        [InlineData(tc01)]
        [InlineData(tc02)]
        [InlineData(tc03)]
        [InlineData(tc04)]
        [InlineData(tc05)]
        [InlineData(tc06)]
        [InlineData(tc07)]
        [InlineData(tc08)]
        [InlineData(tc09)]
        [InlineData(tc10)]
        [InlineData(tc11)]
        [InlineData(tc12)]
        [InlineData(tc13)]
        [InlineData(tc14)]
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
                var hasMessage = mock.Object.Add(item.Data);
                string msg;

                mock.Object.TryTake(out msg).Should().Be(item.ExpectMessage);
                errorRaised.Should().Be(item.ExpectError);
#if DEBUG
                Console.WriteLine(item.Data);
#endif
            }
        }

        [Fact]
        public void SingleBufferOverflow()
        {
            var mock = new Mock<MessageBuffer>();
            var errorRaised = false;

            mock.Object.DataError += (sender, args) =>
            {
                errorRaised = true;
            };

            var msg = MsgToString(message_1);
            while (mock.Object.BufferUsed + msg.Length < MessageBuffer.BufferCapacity)
            {
                mock.Object.Add(msg);
            }
            errorRaised.Should().Be(false, "Since we are not overflowing yet");
            mock.Object.Add(MsgToString(message_1));
            errorRaised.Should().Be(true, "Since we are overflowing");
        }

        [Fact]
        public void AddEmpty()
        {
            var mock = new Mock<MessageBuffer>();
            var errorRaised = false;

            mock.Object.DataError += (sender, args) =>
            {
                errorRaised = true;
            };

            mock.Object.Add(string.Empty);
            errorRaised.Should().Be(false, "Since adding an empty string is not an error");
            mock.Object.Add(null);
            errorRaised.Should().Be(false, "Since adding a null value is not an error");
            mock.Object.BufferUsed.Should().Be(0, "Since we didn't add anyting to the buffer");
        }

        [Fact]
        public void RemovesFirstOnBufferOverflow()
        {
            var mock = new Mock<MessageBuffer>();
            var errorRaised = false;

            mock.Object.DataError += (sender, args) =>
            {
                errorRaised = true;
            };
            mock.Object.Add(MsgToString(message_1));
            var msg2 = MsgToString(message_2);
            while (mock.Object.BufferUsed + msg2.Length < MessageBuffer.BufferCapacity)
            {
                mock.Object.Add(msg2);
            }

            errorRaised.Should().Be(false, "Because the buffer is not yet full");
            mock.Object.Add(MsgToString(message_3));
            errorRaised.Should().Be(true, "Because the buffer is now full");
            errorRaised = false;


            mock.Object.TryTake(out string msg).Should().Be(true);
            msg.Should().BeEquivalentTo(MsgToString(message_2), "Since the first message is dropped");
            string lastMsg = string.Empty;
            while (mock.Object.TryTake(out msg))
            {
                lastMsg = msg;
            }
            lastMsg.Should().BeEquivalentTo(MsgToString(message_3), "Last message should be number three");
            errorRaised.Should().Be(false, "Because there shouldn't be any problem");
        }

        [Fact]
        public void CanHandleLargeDataAddition()
        {
            var mock = new Mock<MessageBuffer>();
            var errorRaised = false;

            mock.Object.DataError += (sender, args) =>
            {
                errorRaised = true;
            };
            var sb = new StringBuilder();
            sb.Append(MsgToString(message_1));  // first message to stringbuilder
            var msg2 = MsgToString(message_2);  // add a bunch of message 2 to the stringbuilder
            for (int i = 0; i < 10; i++)
            {
                sb.Append(msg2);
            }
            sb.Append(MsgToString(message_3));  // and message three to stringbuilder

            mock.Object.Add(sb.ToString()); // add to the buffer

            errorRaised.Should().Be(true, "Because the buffer has overflowed");

            mock.Object.TryTake(out string msg).Should().Be(true, "There should be an message available");
            msg.Should().BeEquivalentTo(MsgToString(message_2), "Since the first message is dropped");
            string lastMsg = string.Empty;
            while (mock.Object.TryTake(out msg))
            {
                lastMsg = msg;
            }
            lastMsg.Should().BeEquivalentTo(MsgToString(message_3), "Last message should be number three");
            mock.Object.IsEmpty.Should().BeTrue("Because we did add only complete messages and have retrieved all off them that did fit in the buffer");
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

        private static string[] message_3 =
        {
            @"/ISK5\2M550T-1013",
            "",
            @"1-3:0.2.8(50)",
            @"0-0:1.0.0(210331213234S)",
            @"0-0:96.1.1(4530303534303037343736393431373139)",
            @"1-0:1.8.1(002478.015*kWh)",
            @"1-0:1.8.2(001898.235*kWh)",
            @"1-0:2.8.1(000825.427*kWh)",
            @"1-0:2.8.2(002015.102*kWh)",
            @"0-0:96.14.0(0002)",
            @"1-0:1.7.0(00.408*kW)",
            @"1-0:2.7.0(00.000*kW)",
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
            @"1-0:32.7.0(229.2*V)",
            @"1-0:52.7.0(230.1*V)",
            @"1-0:72.7.0(228.9*V)",
            @"1-0:31.7.0(001*A)",
            @"1-0:51.7.0(001*A)",
            @"1-0:71.7.0(000*A)",
            @"1-0:21.7.0(00.013*kW)",
            @"1-0:41.7.0(00.269*kW)",
            @"1-0:61.7.0(00.129*kW)",
            @"1-0:22.7.0(00.000*kW)",
            @"1-0:42.7.0(00.000*kW)",
            @"1-0:62.7.0(00.000*kW)",
            @"!A5B3"
        };

        private sealed record TestItem
        {
            public string Data { get; set; } = default!;
            public bool ExpectMessage { get; set; }
            public bool ExpectError { get; set; }
        }
    }
}