using Confluent.Kafka;
using Foundry.Common.Models;
using System.Text.Json;
using System.Xml.Serialization;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<XmlToJson>();
builder.Build().Run();

// XmlPackets (XML) -> Packets (JSON).
// Reads Foundry:Input and writes Foundry:Output; a message it can't handle is logged and skipped.
sealed class XmlToJson(IConfiguration config, IHostEnvironment env, ILogger<XmlToJson> log) : BackgroundService
{
    const string TypeHeader = "foundry-type"; // tells apart the records of a topic carrying several types

    protected override Task ExecuteAsync(CancellationToken ct) => Task.Run(() => Run(ct), ct);

    async Task Run(CancellationToken ct)
    {
        var input = config.GetSection("Foundry:Input");
        var output = config.GetSection("Foundry:Output");
        using var consumer = new ConsumerBuilder<byte[], byte[]>(new ConsumerConfig(Kafka(input))
        {
            GroupId = $"{config["Foundry:Pipeline"] ?? env.EnvironmentName}.{config["Foundry:Processor"] ?? "XmlToJson"}", // locally the environment (prod, load); deployed, the pipeline sets both
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoOffsetStore = false, // stored only after the produce succeeds
        }).Build();
        using var producer = new ProducerBuilder<byte[], byte[]>(new ProducerConfig(Kafka(output)) { EnableIdempotence = true }).Build();
        consumer.Subscribe(input["Topic"]);
        log.LogInformation("{Input} -> {Output}", input["Topic"], output["Topic"]);

        while (!ct.IsCancellationRequested)
        {
            ConsumeResult<byte[], byte[]> result;
            try
            {
                result = consumer.Consume(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e) when (!e.Error.IsFatal)
            {
                log.LogWarning("{Reason}", e.Error.Reason); // e.g. the topic doesn't exist yet
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
                continue;
            }

            Packets value;
            try
            {
                value = Process(Read(result.Message.Value));
            }
            catch (FormatException e)
            {
                log.LogWarning(e, "Skipping bad message at {Offset}", result.TopicPartitionOffset);
                consumer.StoreOffset(result);
                continue;
            }
            var headers = new Headers { { TypeHeader, System.Text.Encoding.UTF8.GetBytes(nameof(Packets)) } };
            await producer.ProduceAsync(output["Topic"], new Message<byte[], byte[]> { Key = result.Message.Key, Value = JsonSerializer.SerializeToUtf8Bytes(value), Headers = headers }, ct);
            consumer.StoreOffset(result);
        }
        consumer.Close();
    }

    // Payload stays base64 encoded: decoding it is the Decode processor's job.
    static Packets Process(XmlPackets p) => new()
    {
        Guid = p.Guid,
        Data = Convert.ToBase64String(p.Data),
        TimeSent = p.TimeSent,
        HostIp = p.HostIp,
        TargetIp = p.TargetIp,
    };

    static readonly XmlSerializer Xml = new(typeof(XmlPackets));

    // Bad XML throws FormatException: skipped.
    static XmlPackets Read(byte[] value)
    {
        try
        {
            return (XmlPackets)Xml.Deserialize(new MemoryStream(value))!;
        }
        catch (InvalidOperationException e)
        {
            throw new FormatException(e.InnerException?.Message ?? e.Message, e);
        }
    }

    // ConnectionSettings from the profile; credentials from Secrets:<SecretRef>:Username/Password (env Secrets__<ref>__Username).
    ClientConfig Kafka(IConfigurationSection dataSet)
    {
        var settings = dataSet.GetSection("ConnectionSettings");
        var client = new ClientConfig { BootstrapServers = settings["Brokers"] };
        if (settings["SecurityProtocol"] is { Length: > 0 } protocol)
            client.Set("security.protocol", protocol);
        if (settings["SaslMechanism"] is { Length: > 0 } mechanism)
            client.Set("sasl.mechanism", mechanism);
        if (settings["SecretRef"] is { Length: > 0 } secret)
        {
            client.SaslUsername = config[$"Secrets:{secret}:Username"];
            client.SaslPassword = config[$"Secrets:{secret}:Password"];
        }
        return client;
    }
}
