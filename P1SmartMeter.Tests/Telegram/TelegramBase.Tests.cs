using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

using P1SmartMeter.Telegram;

namespace P1SmartMeter.Tests.Telegram
{
    public class TelegramBaseTests
    {
        [Fact]
        public void HandlesNullRawData()
        {
            var td = new H();
            var tb = new TelegramBase(td, null);
            tb.Header.Should().BeNullOrWhiteSpace("without raw data, there is no header");
            tb.Crc16.Should().BeNullOrWhiteSpace("without raw data, there is no crc");
            tb.Fields.Count.Should().Be(0, "without raw data, there are no fields");
            tb.ToString().Should().BeNullOrWhiteSpace("without raw data, there there is no string defined");
        }

        [Fact]
        public void HandlesIncorrectRawData()
        {
            const string invalidRawData = "__invalid_raw__";

            var td = new H();
            var tb = new TelegramBase(td, invalidRawData);
            tb.Header.Should().BeNullOrWhiteSpace("with invalid raw data, there is no header");
            tb.Crc16.Should().BeNullOrWhiteSpace("with invalid raw data, there is no crc");
            tb.Fields.Count.Should().Be(1, "with invalid raw data, the data is an invalid field");
            tb.Fields[0].Definition.Should().BeEquivalentTo(TelegramFieldDefinition.Invalid, "with invalid raw data, the data is an invalid field");
            tb.ToString().Should().Be($"invalid: {invalidRawData}{Environment.NewLine}", "without raw data, there there is no string defined");
        }

        [Fact]
        public void HandlesUnknownRawData()
        {
            const string invalidRawData = "0-0:1.0.0(210307123722W)";

            var td = new H();
            var tb = new TelegramBase(td, invalidRawData);
            tb.Header.Should().BeNullOrWhiteSpace("with invalid raw data, there is no header");
            tb.Crc16.Should().BeNullOrWhiteSpace("with invalid raw data, there is no crc");
            tb.Fields.Count.Should().Be(1, "with invalid raw data, the data is an invalid field");
            tb.Fields[0].Definition.Code.Should().BeEquivalentTo("0-0:1.0.0", "with invalid raw data, the data is an invalid field");

            tb.Fields[0].Definition.Types.Count.Should().Be(1, "only one");
            tb.Fields[0].Definition.Types[0].Should().Be(TelegramFieldType.Plain, "this is the default for an unknown field");
            tb.Fields[0].Values.Count.Should().Be(1, "there is only one value");
            tb.Fields[0].Values[0].Should().Be("210307123722W", "is the value supplied for the unknown field");
        }
    }

    public class H : ITelegramDefinition
    {
        public IList<TelegramFieldDefinition> GetFieldDefinitions()
        {
            return new List<TelegramFieldDefinition>();
        }
    }
}
