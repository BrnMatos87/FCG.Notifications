using FCG.BuildingBlocks.Events;
using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Commands.Notifications;
using FCG.Notifications.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Notifications.Tests.Worker.Consumers;

public class UserCreatedConsumerTests
{
    [Fact(DisplayName = "Validando consumo do evento UserCreated")]
    [Trait("Categoria", "Worker - Consumers")]
    public async Task UserCreatedConsumer_Consume_Success()
    {
        var handler = new Mock<ICommandHandlerVoid<SendWelcomeEmailCommand>>();
        var logger = new Mock<ILogger<UserCreatedConsumer>>();
        var context = new Mock<ConsumeContext<UserCreatedEvent>>();

        var message = new UserCreatedEvent
        {
            UserId = Guid.NewGuid(),
            Name = "Bruno",
            Email = "bruno@email.com",
            CreatedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid()
        };

        context.Setup(x => x.Message).Returns(message);
        context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var consumer = new UserCreatedConsumer(handler.Object, logger.Object);

        await consumer.Consume(context.Object);

        handler.Verify(x => x.HandleAsync(
                It.Is<SendWelcomeEmailCommand>(c =>
                    c.UserId == message.UserId &&
                    c.Name == message.Name &&
                    c.Email == message.Email &&
                    c.CorrelationId == message.CorrelationId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}