using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EMS.WebHost.Helpers
{
    public class FloatConverterP0 : JsonConverter<float>
    {
        public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
        public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(string.Format("{0:F0}", value));
        }
    }

    public class FloatConverterP1 : JsonConverter<float>
    {
        public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
        public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(string.Format("{0:F1}", value));
        }
    }
}
