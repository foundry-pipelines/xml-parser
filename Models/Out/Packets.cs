using System.Text.Json.Serialization;

namespace XmlToJsonProcessor.Models.Out;

/// <summary>A packet as JSON, payload still base64.</summary>
public sealed class Packets
{
    [JsonPropertyName("guid")]
    public Guid Guid { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; } = "";

    [JsonPropertyName("time_sent")]
    public DateTimeOffset TimeSent { get; set; }

    [JsonPropertyName("host_ip")]
    public string HostIp { get; set; } = "";

    [JsonPropertyName("target_ip")]
    public string TargetIp { get; set; } = "";
}
