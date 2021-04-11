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
    }

    public class H : ITelegramDefinition
    {
        public IList<TelegramFieldDefinition> GetFieldDefinitions()
        {
            return new List<TelegramFieldDefinition>();
        }
    }
}
