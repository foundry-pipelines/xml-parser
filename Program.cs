using Foundry.Common.Models;
using Foundry.Common.Transformers;

FoundryTransform.Run<XmlPackets, Packets>(args, Convert);

// Payload stays base64 encoded: decoding it is the Decode transform's job.
static Packets Convert(XmlPackets p) => new()
{
    Guid = p.Guid,
    Data = System.Convert.ToBase64String(p.Data),
    TimeSent = p.TimeSent,
    HostIp = p.HostIp,
    TargetIp = p.TargetIp,
};
