using MqttGateway.Server.Objects;
using MqttGateway.Server.Services.Contracts;
using MQTTnet;
using MQTTnet.Protocol;
using System.Text;

namespace MqttGateway.Server.Services;

public class MqttBrokerConnectionHandler : IMqttBrokerConnectionHandler, IMqttMessageDispatcher
{
    private readonly Dictionary<Guid, Guid> _sessionClients = [];
    private readonly MqttConnectionStringBuilder _connectionStringBuilder;
    private IMqttEventDispatcher? _mqttEventDispatcher;
    private readonly IMqttClient mqttClient;

    private const string baseTopic = "personal";

    public MqttBrokerConnectionHandler(
        IConfiguration configuration)
    {
        var mqttConnectionString = configuration.GetConnectionString("MqttBroker");

        _connectionStringBuilder = new()
        {
            ConnectionString = mqttConnectionString ?? throw new ArgumentNullException(nameof(mqttConnectionString))
        };

        var factory = new MqttClientFactory();
        mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_connectionStringBuilder.Server, _connectionStringBuilder.Port)
                .WithCleanSession(_connectionStringBuilder.CleanSession);

        if (_connectionStringBuilder.TrustedConnection.HasValue)
        {
            if (!_connectionStringBuilder.TrustedConnection.Value)
                options = options.WithCredentials(_connectionStringBuilder.User, _connectionStringBuilder.Password); // se tiver auth
            else
                options = options.WithTlsOptions(new MqttClientTlsOptions()
                {

                }); // se tiver TLS
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(_connectionStringBuilder.User))
                options = options.WithCredentials(_connectionStringBuilder.User, _connectionStringBuilder.Password); // se tiver auth
        }

        mqttClient.ApplicationMessageReceivedAsync += HandlerMessageReceivedAsync;
        mqttClient.ConnectAsync(options.Build()).Wait();
    }

    public async Task SubscribeClientAsync(Guid clientId, Guid sessionId, CancellationToken stoppingToken = default)
    {
        if (_sessionClients.ContainsKey(clientId))
            return;

        _sessionClients[sessionId] = clientId;

        try
        {
            await mqttClient.SubscribeAsync(GetTopicBySessionId(sessionId), cancellationToken: stoppingToken);
        }
        catch
        {
            _sessionClients.Remove(clientId);
        }
    }

    public async Task UnsubscribeClientAsync(Guid sessionId, CancellationToken stoppingToken = default)
    {
        if (!_sessionClients.ContainsKey(sessionId))
            return;

        await mqttClient.UnsubscribeAsync(GetTopicBySessionId(sessionId), cancellationToken: stoppingToken);
        _sessionClients.Remove(sessionId);
    }

    public Task PublishMessageAsync(Guid sessionId, string payload, string? channel = null, CancellationToken stoppingToken = default)
    {
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(GetTopicBySessionId(sessionId, channel))
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce) // QoS 2
            .Build();

        return mqttClient.PublishAsync(mqttMessage, stoppingToken);
    }
    public void SetDispatcher(IMqttEventDispatcher dispatcher)
        => _mqttEventDispatcher = dispatcher;

    private Task HandlerMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;
        var payload = args.ApplicationMessage.Payload.Length > 0 ? Encoding.UTF8.GetString(args.ApplicationMessage.Payload) : string.Empty;

        topic = topic[(baseTopic.Length + 1)..];

        var subtopics = topic.Split('/');

        if (subtopics.Length < 2 ||
             !Guid.TryParse(subtopics[1], out Guid sessionId))
            return Task.CompletedTask;

        _mqttEventDispatcher?.DispatchEvent(sessionId, payload, subtopics.Last());

        return Task.CompletedTask;
    }

    private string GetTopicBySessionId(Guid sessionId, string? channel = null)
    {
        Guid clientId = _sessionClients[sessionId];
        string topic = $"{baseTopic}/{clientId}/{sessionId}";
        return string.IsNullOrWhiteSpace(channel) ? topic : $"{topic}/{channel}";
    }
}