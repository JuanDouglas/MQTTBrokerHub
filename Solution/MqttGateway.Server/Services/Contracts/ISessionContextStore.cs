using MqttGateway.Server.Objects;

namespace MqttGateway.Server.Services.Contracts;

public interface ISessionContextStore
{
    SessionContext? GetContext(Guid sessionId);
    bool RemoveContext(Guid sessionId);
    bool CreateContext(Guid sesionId, string start);
}