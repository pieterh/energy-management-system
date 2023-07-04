using System.Xml.Serialization;

using EMS.Library.Xml;

namespace XmlHelpersUnitTests;

public class XmlHelpersTests
{
    [Fact]
    void ParsesSimpleXml()
    {
        string simpleXml =
            """
            <test>
               <name>pieter</name>
            </test>
            """;

        var obj = XmlHelpers.XmlToObject<SimpleClass>(simpleXml);
        obj.Should().NotBeNull();
        obj.Should().BeOfType<SimpleClass>();
        obj.Name.Should().NotBeEmpty();
    }

    [Fact]
    void HandlesQuotes1()
    {
        string simpleXml =
            """
            <test>
               <name>pieter</name>
            </test>
            """;

        var obj = XmlHelpers.XmlToObject<SimpleClass>(simpleXml);
        obj.Should().NotBeNull();
        obj.Should().BeOfType<SimpleClass>();
        obj.Name.Should().NotBeEmpty();
        obj.Name?.First<char>().Should().NotBe('"');
    }

    [Fact]
    void HandlesQuotes2()
    {
        string simpleXml =
            """
            <test>
               <name>&quot;pieter&quot;</name>
            </test>
            """;

        var obj = XmlHelpers.XmlToObject<SimpleClass>(simpleXml);
        obj.Should().NotBeNull();
        obj.Should().BeOfType<SimpleClass>();
        obj.Name.Should().NotBeEmpty();
        obj.Name?.First<char>().Should().Be('"');
    }
}

[XmlRoot("test", Namespace = "", IsNullable = false)]
public class SimpleClass
{
    [XmlElement("name")]
    public string? Name { get; set; }
}

