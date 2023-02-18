using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EMS.WebHost.Helpers
{
    public class FloatConverterP0 : JsonConverter<float>
    {
        private readonly static CultureInfo _cultureInfo = System.Globalization.CultureInfo.GetCultureInfo("en");
        public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
        public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue(string.Format(_cultureInfo, "{0:F0}", value));
        }
    }

    public class FloatConverterP1 : JsonConverter<float>
    {
        private readonly static CultureInfo _cultureInfo =  System.Globalization.CultureInfo.GetCultureInfo("en");
        public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
        public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue(string.Format(_cultureInfo, "{0:F1}", value));
        }
    }
}
