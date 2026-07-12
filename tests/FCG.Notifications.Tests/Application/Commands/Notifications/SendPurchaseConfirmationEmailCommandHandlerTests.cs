using FCG.Notifications.Application.Commands.Notifications;
using FCG.Notifications.Application.Commands.Notifications.Handlers;
using FCG.Notifications.Application.Contracts;
using FCG.Notifications.Domain.Enums;
using FCG.Notifications.Domain.Exceptions;
using FCG.Notifications.Domain.ValueObjects;
using Moq;

namespace FCG.Notifications.Tests.Application.Commands.Notifications;

public class SendPurchaseConfirmationEmailCommandHandlerTests
{
    [Fact(DisplayName = "Validando envio de confirmação de compra")]
    [Trait("Categoria", "Application - Notifications")]
    public async Task SendPurchaseConfirmationEmail_Success()
    {
        var emailSender = new Mock<IEmailSender>();

        var handler = new SendPurchaseConfirmationEmailCommandHandler(emailSender.Object);

        var correlationId = Guid.NewGuid();

        await handler.HandleAsync(new SendPurchaseConfirmationEmailCommand
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "bruno@email.com",
            GameTitle = "Cyber Game",
            Price = 99.90m,
            CorrelationId = correlationId
        });

        emailSender.Verify(x => x.SendAsync(
                It.Is<EmailAddress>(e => e.Value == "bruno@email.com"),
                "Compra confirmada - FIAP Cloud Games",
                It.Is<string>(body =>
                    body.Contains("Cyber Game") &&
                    body.Contains("99")),
                NotificationType.PurchaseConfirmation,
                correlationId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando confirmação de compra com e-mail inválido")]
    [Trait("Categoria", "Application - Notifications")]
    public async Task SendPurchaseConfirmationEmail_InvalidEmail()
    {
        var emailSender = new Mock<IEmailSender>();

        var handler = new SendPurchaseConfirmationEmailCommandHandler(emailSender.Object);

        var result = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new SendPurchaseConfirmationEmailCommand
            {
                OrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                UserEmail = "email-invalido",
                GameTitle = "Cyber Game",
                Price = 99.90m,
                CorrelationId = Guid.NewGuid()
            }));

        Assert.Equal("Formato de e-mail inválido.", result.Message);

        emailSender.Verify(x => x.SendAsync(
                It.IsAny<EmailAddress>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}