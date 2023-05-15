using System.Xml;
using System.Xml.Serialization;

namespace EMS.Library.Xml;

public static class XmlHelpers
{
    public static T XmlToObject<T>(string xml)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader);
        return (T)xmlSerializer.Deserialize(xmlReader)!;
    }
}