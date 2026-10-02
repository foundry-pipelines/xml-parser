using Foundry.Models;
using Foundry.Models.Configuration;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = args, ContentRootPath = AppContext.BaseDirectory });
var foundry = builder.Configuration.GetSection(FoundryOptions.SectionName).Get<FoundryOptions>()
    ?? throw new InvalidOperationException("Missing Foundry configuration");
foundry.EnsureSchemas<XmlPackets, Packets>();

builder.Services.AddHostedService(sp => new TransformWorker<XmlPackets, Packets>(
    foundry, builder.Configuration, sp.GetRequiredService<ILogger<TransformWorker<XmlPackets, Packets>>>(), Convert));
builder.Build().Run();

// Payload stays base64 encoded: decoding it is the Decode transform's job.
static Packets Convert(XmlPackets p) => new()
{
    Guid = p.Guid,
    Data = System.Convert.ToBase64String(p.Data),
    TimeSent = p.TimeSent,
    HostIp = p.HostIp,
    TargetIp = p.TargetIp,
};
