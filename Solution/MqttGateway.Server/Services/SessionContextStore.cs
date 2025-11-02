using MqttGateway.Server.Objects;
using MqttGateway.Server.Services.Contracts;

namespace MqttGateway.Server.Services;

public class SessionContextStore : ISessionContextStore
{
    private readonly Dictionary<Guid, SessionContext> _sessions;

    public SessionContextStore()
    {
        _sessions = new Dictionary<Guid, SessionContext>();
    }

    public bool CreateContext(Guid sessionId, string start)
    {
        if (_sessions.ContainsKey(sessionId))
            return false;

        _sessions.Add(sessionId, new SessionContext(start));
        return true;
    }

    public SessionContext? GetContext(Guid sessionId)
    {
        if (!_sessions.ContainsKey(sessionId))
            return null;

        return _sessions[sessionId];
    }

    public bool RemoveContext(Guid sessionId)
    {
        if (!_sessions.ContainsKey(sessionId))
            return false;

        return _sessions.Remove(sessionId);
    }
}