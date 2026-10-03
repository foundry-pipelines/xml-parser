using System.Net;
using System.Text.Json.Serialization;
using Foundry.Common.Serialization;

namespace XmlToJsonProcessor.Types;

/// <summary>A packet as JSON, payload still base64.</summary>
public sealed class Packets
{
    [JsonPropertyName("guid")]
    public Guid Guid { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; } = "";

    [JsonPropertyName("time_sent")]
    public DateTimeOffset TimeSent { get; set; }

    [JsonPropertyName("host_ip"), JsonConverter(typeof(IpAddressJsonConverter))]
    public IPAddress HostIp { get; set; } = IPAddress.None;

    [JsonPropertyName("target_ip"), JsonConverter(typeof(IpAddressJsonConverter))]
    public IPAddress TargetIp { get; set; } = IPAddress.None;
}
