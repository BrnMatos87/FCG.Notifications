using System.Text.Json;
using FCG.BuildingBlocks.Enums;
using FCG.BuildingBlocks.Events;
using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Commands.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "post",
            Route = "notifications/payment-processed")]
        HttpRequest request,
        FunctionContext context)
    {
        PaymentProcessedEvent? paymentProcessedEvent;

        try
        {
            paymentProcessedEvent = await request.ReadFromJsonAsync<PaymentProcessedEvent>(
                context.CancellationToken);
        }
        catch (Exception exception) when (
            exception is BadHttpRequestException or JsonException)
        {
            _logger.LogWarning(exception, "Payload de notificação de pagamento inválido.");
            return new BadRequestObjectResult("O corpo da requisição contém JSON inválido.");
        }

        if (paymentProcessedEvent is null ||
            paymentProcessedEvent.OrderId == Guid.Empty ||
            paymentProcessedEvent.UserId == Guid.Empty ||
            paymentProcessedEvent.GameId == Guid.Empty ||
            string.IsNullOrWhiteSpace(paymentProcessedEvent.UserEmail) ||
            string.IsNullOrWhiteSpace(paymentProcessedEvent.GameTitle) ||
            paymentProcessedEvent.CorrelationId == Guid.Empty ||
            !Enum.IsDefined(paymentProcessedEvent.Status))
        {
            _logger.LogWarning(
                "Payload de notificação de pagamento não contém todos os campos obrigatórios.");

            return new BadRequestObjectResult(
                "OrderId, UserId, GameId, UserEmail, GameTitle, Status e CorrelationId são obrigatórios.");
        }

        _logger.LogInformation(
            "Notificação de pagamento recebida. OrderId: {OrderId}, UserId: {UserId}, Status: {Status}, CorrelationId: {CorrelationId}",
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

            return new NoContentResult();
        }

        _logger.LogInformation(
            "Pagamento foi aprovado. Notificação será enviada. OrderId: {OrderId}, Status: {Status}, CorrelationId: {CorrelationId}",
            paymentProcessedEvent.OrderId,
            paymentProcessedEvent.Status,
            paymentProcessedEvent.CorrelationId);

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

        return new NoContentResult();
    }
}
