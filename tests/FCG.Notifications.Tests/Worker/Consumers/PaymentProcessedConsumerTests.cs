using FCG.BuildingBlocks.Enums;
using FCG.BuildingBlocks.Events;
using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Commands.Notifications;
using FCG.Notifications.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Notifications.Tests.Worker.Consumers;

public class PaymentProcessedConsumerTests
{
    [Fact(DisplayName = "Validando consumo de pagamento aprovado")]
    [Trait("Categoria", "Worker - Consumers")]
    public async Task PaymentProcessedConsumer_Approved_Success()
    {
        var handler = new Mock<ICommandHandlerVoid<SendPurchaseConfirmationEmailCommand>>();
        var logger = new Mock<ILogger<PaymentProcessedConsumer>>();
        var context = new Mock<ConsumeContext<PaymentProcessedEvent>>();

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

        context.Setup(x => x.Message).Returns(message);
        context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var consumer = new PaymentProcessedConsumer(handler.Object, logger.Object);

        await consumer.Consume(context.Object);

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

    [Fact(DisplayName = "Validando consumo de pagamento rejeitado")]
    [Trait("Categoria", "Worker - Consumers")]
    public async Task PaymentProcessedConsumer_Rejected_ShouldNotSendEmail()
    {
        var handler = new Mock<ICommandHandlerVoid<SendPurchaseConfirmationEmailCommand>>();
        var logger = new Mock<ILogger<PaymentProcessedConsumer>>();
        var context = new Mock<ConsumeContext<PaymentProcessedEvent>>();

        var message = new PaymentProcessedEvent
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "bruno@email.com",
            GameTitle = "Cyber Game",
            Price = 99.90m,
            Status = PaymentStatus.Rejected,
            ProcessedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid()
        };

        context.Setup(x => x.Message).Returns(message);
        context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var consumer = new PaymentProcessedConsumer(handler.Object, logger.Object);

        await consumer.Consume(context.Object);

        handler.Verify(x => x.HandleAsync(
                It.IsAny<SendPurchaseConfirmationEmailCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}