using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Enphase.DTO.Info;

// clases are publice since we are using the XmlSerializer class. DataContractSerializer doesn't give the flexibility
// to handle repeating elements (like the collection of Packages)

// disable these warnings, since the XML deserializer will validate anyway that they are set properly
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

[XmlRoot("envoy_info", Namespace = "", IsNullable = false)]
[KnownType(typeof(Device))]
[KnownType(typeof(Package))]
[KnownType(typeof(BuildInfo))]
public class InfoResponse
{
    [XmlElement("time")]
    public int Time { get; set; }

    [XmlElement("device")]
    public Device Device { get; set; }

    [XmlElement("package")]
    public Collection<Package> Packages { get; } = new Collection<Package>();

    [XmlElement("build_info")]
    public BuildInfo BuildInfo { get; set; }
}

public record Device
{
    [XmlElement("sn")]
    public string SerialNumber { get; set; }

    [XmlElement("pn")]
    public string PartNumber { get; set; }

    [XmlElement("software")]
    public string Software { get; set; }

    [XmlElement("euaid")]
    public string euaid { get; set; }

    [XmlElement("seqnum")]
    public string SeqNumber { get; set; }

    [XmlElement("apiver")]
    public string ApiVersion { get; set; }

    [XmlElement("imeter")]
    public string IMeter { get; set; }
}

public record Package
{
    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlElement("pn")]
    public string pn { get; set; }

    [XmlElement("version")]
    public string Version { get; set; }

    [XmlElement("build")]
    public string Build { get; set; }
}

public record BuildInfo
{
    [XmlElement("build_id")]
    public string BuildId { get; set; }

    [XmlElement("build_time_gmt")]
    public string BuildTimeGmt { get; set; }

    public DateTimeOffset BuildDate => DateTimeOffset.FromUnixTimeSeconds(long.Parse(BuildTimeGmt));
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.