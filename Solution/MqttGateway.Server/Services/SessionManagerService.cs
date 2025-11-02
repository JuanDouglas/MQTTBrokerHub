using MqttGateway.Server.Services.Contracts;

namespace MqttGateway.Server.Services;

public class SessionManagerService : ISessionManager
{
    private readonly List<Guid> _sessions;
    private readonly Dictionary<Guid, HashSet<string>> _relays;
    private readonly IMqttBrokerConnectionHandler _mqttConnectionHandler;
    private readonly ISessionContextStore _sessionContextStore;
    public SessionManagerService(
        IMqttBrokerConnectionHandler mqttBrokerConnectionHandler,
        ISessionContextStore sessionContextStore)
    {
        _mqttConnectionHandler = mqttBrokerConnectionHandler;
        _sessionContextStore = sessionContextStore;
        _sessions = [];
        _relays = [];
    }

    public HashSet<string> RelayClients(Guid sessionId)
    {
        if (!ExistsSession(sessionId))
            return [];

        return _relays[sessionId];
    }

    public async Task<bool> RemoveConnectionAsync(Guid sessionId, string connectionId, CancellationToken stoppingToken = default)
    {
        if (!ExistsSession(sessionId))
            return false;

        HashSet<string> relayedConnections = _relays[sessionId];
        bool removed = relayedConnections.Remove(connectionId);

        if (relayedConnections.Count < 1)
        {
            await _mqttConnectionHandler.UnsubscribeClientAsync(sessionId, stoppingToken);
            _sessionContextStore.RemoveContext(sessionId);
            _sessions.Remove(sessionId);
        }

        return removed;
    }

    public async Task<bool> SubscribeContext(Guid sessionId, string connectionId, CancellationToken stoppingToken = default)
    {
        if (!ExistsSession(sessionId))
        {
            await _mqttConnectionHandler.SubscribeClientAsync(Guid.NewGuid(), sessionId, stoppingToken);

            _relays[sessionId] = [];
            _sessions.Add(sessionId);
        }

        HashSet<string> relayedConnections = _relays[sessionId];
        return relayedConnections.Add(connectionId);
    }

    private bool ExistsSession(Guid sessionId)
    {
        return _sessions.Contains(sessionId);
    }
}