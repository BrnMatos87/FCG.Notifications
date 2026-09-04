using System.Text.Json;
using FCG.BuildingBlocks.Events;
using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Commands.Notifications;
using FCG.Notifications.Functions.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FCG.Notifications.Functions.Functions;

public class UserCreatedFunction
{
    private readonly ICommandHandlerVoid<SendWelcomeEmailCommand> _handler;
    private readonly ILogger<UserCreatedFunction> _logger;

    public UserCreatedFunction(
        ICommandHandlerVoid<SendWelcomeEmailCommand> handler,
        ILogger<UserCreatedFunction> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [Function("UserCreatedFunction")]
    public async Task RunAsync(
        [RabbitMQTrigger(
            "notifications-user-created",
            ConnectionStringSetting = "RabbitMqConnection")]
        string message,
        FunctionContext context)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var envelope =
            JsonSerializer.Deserialize<
                MassTransitEnvelope<UserCreatedEvent>>(
                message,
                options);

        var userCreatedEvent = envelope?.Message;

        if (userCreatedEvent is null)
        {
            _logger.LogWarning(
                "Não foi possível extrair UserCreatedEvent do envelope MassTransit.");

            return;
        }

        _logger.LogInformation(
            "UserCreatedEvent recebido. UserId: {UserId}, Email: {Email}, CorrelationId: {CorrelationId}",
            userCreatedEvent.UserId,
            userCreatedEvent.Email,
            userCreatedEvent.CorrelationId);

        await _handler.HandleAsync(
            new SendWelcomeEmailCommand
            {
                UserId = userCreatedEvent.UserId,
                Name = userCreatedEvent.Name,
                Email = userCreatedEvent.Email,
                CorrelationId = userCreatedEvent.CorrelationId
            },
            context.CancellationToken);
    }
}