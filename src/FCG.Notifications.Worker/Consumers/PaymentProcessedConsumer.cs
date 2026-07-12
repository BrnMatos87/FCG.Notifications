using FCG.BuildingBlocks.Enums;
using FCG.BuildingBlocks.Events;
using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Commands.Notifications;
using MassTransit;

namespace FCG.Notifications.Worker.Consumers;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly ICommandHandlerVoid<SendPurchaseConfirmationEmailCommand> _handler;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(
        ICommandHandlerVoid<SendPurchaseConfirmationEmailCommand> handler,
        ILogger<PaymentProcessedConsumer> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "PaymentProcessedEvent recebido. OrderId: {OrderId}, UserId: {UserId}, Status: {Status}, CorrelationId: {CorrelationId}",
            message.OrderId,
            message.UserId,
            message.Status,
            message.CorrelationId);

        if (message.Status != PaymentStatus.Approved)
        {
            _logger.LogInformation(
                "Pagamento não foi aprovado. Notificação não será enviada. OrderId: {OrderId}, Status: {Status}, CorrelationId: {CorrelationId}",
                message.OrderId,
                message.Status,
                message.CorrelationId);

            return;
        }

        await _handler.HandleAsync(
            new SendPurchaseConfirmationEmailCommand
            {
                OrderId = message.OrderId,
                UserId = message.UserId,
                GameId = message.GameId,
                UserEmail = message.UserEmail,
                GameTitle = message.GameTitle,
                Price = message.Price,
                CorrelationId = message.CorrelationId
            },
            context.CancellationToken);
    }
}