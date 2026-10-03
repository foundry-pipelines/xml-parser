using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace XmlToJsonProcessor.Models.Out;

/// <summary>A raw packet as XML (<c>&lt;xml_packets&gt;</c>), payload base64.</summary>
/// <remarks>The JSON attributes only shape its registered JSON Schema like the XML: it travels as XML.</remarks>
[XmlRoot("xml_packets")]
public sealed class XmlPackets
{
    [XmlElement("guid"), JsonPropertyName("guid")]
    public Guid Guid { get; set; }

    [XmlElement("data", DataType = "base64Binary"), JsonPropertyName("data")]
    public byte[] Data { get; set; } = [];

    [XmlElement("time_sent"), JsonPropertyName("time_sent")]
    public DateTimeOffset TimeSent { get; set; }

    [XmlElement("host_ip"), JsonPropertyName("host_ip")]
    public string HostIp { get; set; } = "";

    [XmlElement("target_ip"), JsonPropertyName("target_ip")]
    public string TargetIp { get; set; } = "";
}
