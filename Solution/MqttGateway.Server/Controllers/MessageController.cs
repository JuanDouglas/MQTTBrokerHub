using Microsoft.AspNetCore.Mvc;
using MqttGateway.Server.Services.Contracts;

namespace MqttGateway.Server.Controllers;

[Route("Messages")]
public class MessageController : ControllerBase
{
    private readonly IMqttMessageDispatcher _mqttDispatcher;

    public MessageController(
        IMqttMessageDispatcher mqttMessageDispatcher)
    {
        _mqttDispatcher = mqttMessageDispatcher;
    }

    [HttpPost("Send")]
    public void SendMessage(Guid sessionId, string message, string? channel = null)
    {
        _mqttDispatcher.PublishMessageAsync(sessionId, message, channel);
    }
}