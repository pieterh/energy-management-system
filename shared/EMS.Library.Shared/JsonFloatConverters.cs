using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EMS.Library.Shared;

public class FloatConverterP0 : JsonConverter<float>
{
    private readonly static CultureInfo _cultureInfo = CultureInfo.GetCultureInfo("en");
    public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var d = reader.GetDouble();
        float f = (float)Math.Round(d, 0);
        return f;
    }

    public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteRawValue(string.Format(_cultureInfo, "{0:F0}", value));
    }
}

public class FloatConverterP1 : JsonConverter<float>
{
    private readonly static CultureInfo _cultureInfo = CultureInfo.GetCultureInfo("en");
    public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var d = reader.GetDouble();
        float f = (float)d;
        return f;
    }

    public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteRawValue(string.Format(_cultureInfo, "{0:F1}", value));
    }
}
