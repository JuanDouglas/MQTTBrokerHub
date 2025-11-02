namespace MqttGateway.Server.Objects;

public class SessionContext
{
    readonly List<History> histories;

    public SessionContext(string start)
    {
        histories = [new History(start)];
    }

    public void IncressPayload(string payload, string? channel = null)
    {
        var history = new History(payload, channel);

        histories.Add(history);
    }
}

public readonly struct History
{
    public string Payload { get; }
    public string? Channel { get; }

    public History(string payload, string? channel = null) : this()
    {
        Payload = payload;
        Channel = channel;
    }
}