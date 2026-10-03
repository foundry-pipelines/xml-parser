using System.ComponentModel;
using System.Net;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Foundry.Common.Serialization;

namespace XmlToJsonProcessor.Types;

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

    [XmlIgnore, JsonPropertyName("host_ip"), JsonConverter(typeof(IpAddressJsonConverter))]
    public IPAddress HostIp { get; set; } = IPAddress.None;

    [XmlIgnore, JsonPropertyName("target_ip"), JsonConverter(typeof(IpAddressJsonConverter))]
    public IPAddress TargetIp { get; set; } = IPAddress.None;

    // XmlSerializer can't handle IPAddress: these carry its string form.
    [XmlElement("host_ip"), JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
    public string HostIpXml { get => HostIp.ToString(); set => HostIp = IPAddress.Parse(value); }

    [XmlElement("target_ip"), JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
    public string TargetIpXml { get => TargetIp.ToString(); set => TargetIp = IPAddress.Parse(value); }
}
