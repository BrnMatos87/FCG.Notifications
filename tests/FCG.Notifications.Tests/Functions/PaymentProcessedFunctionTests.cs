using System.Text.Json;
using FCG.BuildingBlocks.Enums;
using FCG.BuildingBlocks.Events;
using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Commands.Notifications;
using FCG.Notifications.Functions.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Notifications.Tests.Functions;

public class PaymentProcessedFunctionTests
{
    [Fact(DisplayName = "Validando processamento de pagamento aprovado")]
    [Trait("Categoria", "Functions - PaymentProcessed")]
    public async Task PaymentProcessedFunction_Approved_Success()
    {
        var handler = new Mock<ICommandHandlerVoid<SendPurchaseConfirmationEmailCommand>>();
        var logger = new Mock<ILogger<PaymentProcessedFunction>>();
        var context = new Mock<FunctionContext>();

        context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var message = new PaymentProcessedEvent
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "bruno@email.com",
            GameTitle = "Cyber Game",
            Price = 99.90m,
            Status = PaymentStatus.Approved,
            ProcessedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid()
        };

        var request = CreateRequest(message);

        var function = new PaymentProcessedFunction(handler.Object, logger.Object);

        var result = await function.RunAsync(request, context.Object);

        Assert.IsType<NoContentResult>(result);

        handler.Verify(x => x.HandleAsync(
                It.Is<SendPurchaseConfirmationEmailCommand>(c =>
                    c.OrderId == message.OrderId &&
                    c.UserId == message.UserId &&
                    c.GameId == message.GameId &&
                    c.UserEmail == message.UserEmail &&
                    c.GameTitle == message.GameTitle &&
                    c.Price == message.Price &&
                    c.CorrelationId == message.CorrelationId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando processamento de pagamento rejeitado")]
    [Trait("Categoria", "Functions - PaymentProcessed")]
    public async Task PaymentProcessedFunction_Rejected_ShouldNotSendEmail()
    {
        var handler = new Mock<ICommandHandlerVoid<SendPurchaseConfirmationEmailCommand>>();
        var logger = new Mock<ILogger<PaymentProcessedFunction>>();
        var context = new Mock<FunctionContext>();

        context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var message = new PaymentProcessedEvent
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "bruno@email.com",
            GameTitle = "Cyber Game",
            Price = 99m,
            Status = PaymentStatus.Rejected,
            ProcessedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid()
        };

        var request = CreateRequest(message);

        var function = new PaymentProcessedFunction(handler.Object, logger.Object);

        var result = await function.RunAsync(request, context.Object);

        Assert.IsType<NoContentResult>(result);

        handler.Verify(x => x.HandleAsync(
                It.IsAny<SendPurchaseConfirmationEmailCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Validando mensagem inválida sem envelope")]
    [Trait("Categoria", "Functions - PaymentProcessed")]
    public async Task PaymentProcessedFunction_InvalidEnvelope_ShouldNotSendEmail()
    {
        var handler = new Mock<ICommandHandlerVoid<SendPurchaseConfirmationEmailCommand>>();
        var logger = new Mock<ILogger<PaymentProcessedFunction>>();
        var context = new Mock<FunctionContext>();

        context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var function = new PaymentProcessedFunction(handler.Object, logger.Object);

        var result = await function.RunAsync(CreateRequest(new { }), context.Object);

        Assert.IsType<BadRequestObjectResult>(result);

        handler.Verify(x => x.HandleAsync(
                It.IsAny<SendPurchaseConfirmationEmailCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static HttpRequest CreateRequest<T>(T body)
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(body));
        return context.Request;
    }
}
