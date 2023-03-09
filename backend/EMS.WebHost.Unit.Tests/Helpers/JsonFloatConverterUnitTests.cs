using System.Buffers;
using System.Text.Json;
using EMS.WebHost.Helpers;
using FluentAssertions;
namespace EMS.WebHost.Unit.Tests;



public class JsonFloatConverterUnitTests
{
    [Fact(DisplayName = "FloatConverterP0 will throw NotImplementedException when trying to read")]
    public void FloatConverterP0NotImplementedException()
    {
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var t = new FloatConverterP0();
        var buffer = new ArrayBufferWriter<byte>();
       
        Action write = () => {
            var reader = new Utf8JsonReader();
            t.Read(ref reader, typeof(float), serializerDefaults);
        };

        write.Should().Throw<NotImplementedException>();
    }
    [Fact(DisplayName = "FloatConverterP0 will throw ArgumentNullException when no writer supplied")]
    public void FloatConverterP0ArgumentNullException()
    {
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var t = new FloatConverterP0();
        Action write = () => t.Write(null, 0, serializerDefaults);
        
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
        buffer.WrittenSpan.SequenceCompareTo("\"0\""u8).Should().Be(0, "result should be \"0\".");
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
        buffer.WrittenSpan.SequenceCompareTo("\"12\""u8).Should().Be(0, "result should be \"12\".");
    }
    [Fact(DisplayName = "FloatConverterP1 will throw NotImplementedException when trying to read")]
    public void FloatConverterP1NotImplementedException()
    {
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var t = new FloatConverterP1();
        var buffer = new ArrayBufferWriter<byte>();

        Action write = () => {
            var reader = new Utf8JsonReader();
            t.Read(ref reader, typeof(float), serializerDefaults);
        };

        write.Should().Throw<NotImplementedException>();
    }
    [Fact(DisplayName = "FloatConverterP1 will throw ArgumentNullException when no writer supplied")]
    public void FloatConverterP1ArgumentNullException()
    {
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var t = new FloatConverterP1();
        Action write = () => t.Write(null, 0, serializerDefaults);

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
        buffer.WrittenSpan.SequenceCompareTo("\"0.0\""u8).Should().Be(0, "result should be \"0.0\".");
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
        buffer.WrittenSpan.SequenceCompareTo("\"12.3\""u8).Should().Be(0, "result should be \"12.3\".");
    }
}

