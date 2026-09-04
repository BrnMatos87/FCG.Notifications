using System.Text.Json;
using FCG.BuildingBlocks.Enums;
using FCG.BuildingBlocks.Events;
using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Commands.Notifications;
using FCG.Notifications.Functions.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FCG.Notifications.Functions.Functions;

public class PaymentProcessedFunction
{
    private readonly ICommandHandlerVoid<SendPurchaseConfirmationEmailCommand> _handler;
    private readonly ILogger<PaymentProcessedFunction> _logger;

    public PaymentProcessedFunction(
        ICommandHandlerVoid<SendPurchaseConfirmationEmailCommand> handler,
        ILogger<PaymentProcessedFunction> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [Function("PaymentProcessedFunction")]
    public async Task RunAsync(
        [RabbitMQTrigger(
            "notifications-payment-processed",
            ConnectionStringSetting = "RabbitMqConnection")]
        string message,
        FunctionContext context)
    {
        _logger.LogInformation(
            "Mensagem bruta recebida: {Message}",
            message);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var envelope =
            JsonSerializer.Deserialize<
                MassTransitEnvelope<PaymentProcessedEvent>>(
                message,
                options);

        var paymentProcessedEvent = envelope?.Message;

        if (paymentProcessedEvent is null)
        {
            _logger.LogWarning(
                "Não foi possível extrair PaymentProcessedEvent do envelope MassTransit.");

            return;
        }

        _logger.LogInformation(
            "PaymentProcessedEvent recebido. OrderId: {OrderId}, UserId: {UserId}, Status: {Status}, CorrelationId: {CorrelationId}",
            paymentProcessedEvent.OrderId,
            paymentProcessedEvent.UserId,
            paymentProcessedEvent.Status,
            paymentProcessedEvent.CorrelationId);

        if (paymentProcessedEvent.Status != PaymentStatus.Approved)
        {
            _logger.LogInformation(
                "Pagamento não foi aprovado. Notificação não será enviada. OrderId: {OrderId}, Status: {Status}, CorrelationId: {CorrelationId}",
                paymentProcessedEvent.OrderId,
                paymentProcessedEvent.Status,
                paymentProcessedEvent.CorrelationId);

            return;
        }

        await _handler.HandleAsync(
            new SendPurchaseConfirmationEmailCommand
            {
                OrderId = paymentProcessedEvent.OrderId,
                UserId = paymentProcessedEvent.UserId,
                GameId = paymentProcessedEvent.GameId,
                UserEmail = paymentProcessedEvent.UserEmail,
                GameTitle = paymentProcessedEvent.GameTitle,
                Price = paymentProcessedEvent.Price,
                CorrelationId = paymentProcessedEvent.CorrelationId
            },
            context.CancellationToken);
    }
}