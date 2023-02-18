using System.Buffers;
using System.Text.Json;
using EMS.WebHost.Helpers;

namespace EMS.WebHost.Unit.Tests;



public class JsonFloatConverterUnitTests
{
    [Fact(DisplayName = "FloatConverterP0 will output 0 without decimal points")]    
    public void FloatConverterP0First()
    {
        JsonSerializerOptions serializerDefaults = new(JsonSerializerDefaults.General);
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);

        var t = new FloatConverterP0();
        t.Write(writer, 0, serializerDefaults);
        writer.Flush();
        Assert.Equal(0, writer.BytesPending);
        Assert.Equal(0, buffer.WrittenSpan.SequenceCompareTo("\"0\""u8));
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
        Assert.Equal(0, writer.BytesPending);
        Assert.Equal(0, buffer.WrittenSpan.SequenceCompareTo("\"12\""u8));
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
        Assert.Equal(0, writer.BytesPending);
        Assert.Equal(0, buffer.WrittenSpan.SequenceCompareTo("\"0.0\""u8));
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
        Assert.Equal(0, writer.BytesPending);
        Assert.Equal(0, buffer.WrittenSpan.SequenceCompareTo("\"12.3\""u8));
    }
}

