using System.Buffers;
using System.Text;
using System.Text.Json;

using EMS.Library.Shared;
using EMS.Library.Shared.DTO.EVSE;

namespace EMS.Library.Shared.Unit.Tests;

public class JsonFloatConverterUnitTests
{
    [Fact(DisplayName = "FloatConverterP0 is able to read float without decimals")]
    public void FloatConverterP0ReadsWithoutDecimal()
    {
        var strToConvert = "{ \"myFloatProp\": 16 }";
        var bytes = Encoding.ASCII.GetBytes(strToConvert);
        var reader = new Utf8JsonReader(bytes);

        // prepare the reader
        reader.TokenType.Should().Be(JsonTokenType.None);
        reader.Read();
        reader.TokenType.Should().Be(JsonTokenType.StartObject);
        reader.Read();
        reader.TokenType.Should().Be(JsonTokenType.PropertyName);
        reader.Read();
        reader.TokenType.Should().Be(JsonTokenType.Number);

        var t = new FloatConverterP0();
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var result = t.Read(ref reader, typeof(float), serializerDefaults);
        result.Should().Be(16.0f);
    }

    [Fact(DisplayName = "FloatConverterP0 is able to read float with decimal and rounds it")]
    public void FloatConverterP0ReadsWithDecimalAndRoundsIt()
    {
        var strToConvert = "{ \"myFloatProp\": 14.9 }";
        var bytes = Encoding.ASCII.GetBytes(strToConvert);
        var reader = new Utf8JsonReader(bytes);

        // prepare the reader
        reader.TokenType.Should().Be(JsonTokenType.None);
        reader.Read();
        reader.TokenType.Should().Be(JsonTokenType.StartObject);
        reader.Read();
        reader.TokenType.Should().Be(JsonTokenType.PropertyName);
        reader.Read();
        reader.TokenType.Should().Be(JsonTokenType.Number);

        var t = new FloatConverterP0();
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var result = t.Read(ref reader, typeof(float), serializerDefaults);
        result.Should().Be(15.0f);
    }

    [Fact(DisplayName = "FloatConverterP0 will throw ArgumentNullException when no writer supplied")]
    public void FloatConverterP0ArgumentNullException()
    {
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var t = new FloatConverterP0();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Action write = () => t.Write(null, 0, serializerDefaults);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        write.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "FloatConverterP0 will output 0 without decimal points")]
    public void FloatConverterP0First()
    {
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);

        var t = new FloatConverterP0();
        t.Write(writer, (float)0.0, serializerDefaults);
        writer.Flush();
        writer.BytesPending.Should().Be(0, "data has converted and flushed");
        buffer.WrittenSpan.SequenceCompareTo("0"u8).Should().Be(0, "result should be \"0\".");
    }

    [Fact(DisplayName = "FloatConverterP0 will output 12.345 without decimal points")]
    public void FloatConverterP0Second()
    {
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);

        var t = new FloatConverterP0();
        t.Write(writer, (float)12.345, serializerDefaults);
        writer.Flush();
        writer.BytesPending.Should().Be(0, "data has converted and flushed");
        buffer.WrittenSpan.SequenceCompareTo("12"u8).Should().Be(0, "result should be \"12\".");
    }

    [Fact(DisplayName = "FloatConverterP1 is able to read float")]
    public void FloatConverterP1NotImplementedException()
    {
        var strToConvert = "{ \"myFloatProp\": 16.1 }";
        var bytes = Encoding.ASCII.GetBytes(strToConvert);
        var reader = new Utf8JsonReader(bytes);

        // prepare the reader
        reader.TokenType.Should().Be(JsonTokenType.None);
        reader.Read();
        reader.TokenType.Should().Be(JsonTokenType.StartObject);
        reader.Read();
        reader.TokenType.Should().Be(JsonTokenType.PropertyName);
        reader.Read();
        reader.TokenType.Should().Be(JsonTokenType.Number);

        var t = new FloatConverterP1();
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var result = t.Read(ref reader, typeof(float), serializerDefaults);
        result.Should().Be(16.1f);
    }

    [Fact(DisplayName = "FloatConverterP1 will throw ArgumentNullException when no writer supplied")]
    public void FloatConverterP1ArgumentNullException()
    {
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var t = new FloatConverterP1();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Action write = () => t.Write(null, 0, serializerDefaults);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        write.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "FloatConverterP1 will output 0 with one decimal point")]
    public void FloatConverterP1First()
    {
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);

        var t = new FloatConverterP1();
        t.Write(writer, 0, serializerDefaults);
        writer.Flush();
        writer.BytesPending.Should().Be(0, "data has converted and flushed");
        buffer.WrittenSpan.SequenceCompareTo("0.0"u8).Should().Be(0, "result should be \"0.0\".");
    }

    [Fact(DisplayName = "FloatConverterP1 will output 12.345 with one decimal point")]
    public void FloatConverterP1Second()
    {
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);

        var t = new FloatConverterP1();
        t.Write(writer, (float)12.345, serializerDefaults);
        writer.Flush();
        writer.BytesPending.Should().Be(0, "data has converted and flushed");
        buffer.WrittenSpan.SequenceCompareTo("12.3"u8).Should().Be(0, "result should be \"12.3\".");
    }

    [Fact]
    public void DeserializesProperlyASocketInfoModel()
    {
        var jsonString =
            """
            {
            		"id": 1,
            		"voltageFormatted": "240.9 V",
            		"currentFormatted": "0.0 A",
            		"realPowerSumFormatted": "586729496115803970251425226686464.0 kW",
            		"realEnergyDeliveredFormatted": "8685 kW",
            		"availability": true,
            		"mode3State": "E",
            		"mode3StateMessage": "No Power",
            		"lastChargingStateChanged": "2023-04-29T16:09:07.432426+02:00",
            		"vehicleIsConnected": false,
            		"vehicleIsCharging": false,
            		"appliedMaxCurrent": 15.1,
            		"maxCurrentValidTime": 49,
            		"maxCurrent": 15.4,
            		"activeLBSafeCurrent": 15.9,
            		"setPointAccountedFor": true,
            		"phases": 1,
            		"powerAvailableFormatted": "11.6 kW",
            		"powerUsingFormatted": "0.0 kW"
            	}
            """u8;

        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.Web);
        var t = JsonSerializer.Deserialize<SocketInfoModel>(jsonString, serializerDefaults);
        Assert.NotNull(t);
        t.AppliedMaxCurrent.Should<float>().Be(15.1f);
        t.MaxCurrentValidTime.Should<UInt32>().Be(49);  // not a float, but we check it anyway
        t.MaxCurrent.Should<float>().Be(15.4f);
        t.ActiveLBSafeCurrent.Should<float>().Be(16f, "it should be rounded");
    }
}