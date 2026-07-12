using FCG.BuildingBlocks.Events;
using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Commands.Notifications;
using MassTransit;

namespace FCG.Notifications.Worker.Consumers;

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly ICommandHandlerVoid<SendWelcomeEmailCommand> _handler;
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(
        ICommandHandlerVoid<SendWelcomeEmailCommand> handler,
        ILogger<UserCreatedConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "UserCreatedEvent recebido. UserId: {UserId}, Email: {Email}, CorrelationId: {CorrelationId}",
            message.UserId,
            message.Email,
            message.CorrelationId);

        await _handler.HandleAsync(
            new SendWelcomeEmailCommand
            {
                UserId = message.UserId,
                Name = message.Name,
                Email = message.Email,
                CorrelationId = message.CorrelationId
            },
            context.CancellationToken);
    }
}