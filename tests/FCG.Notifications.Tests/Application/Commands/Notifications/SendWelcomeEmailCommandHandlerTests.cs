using FCG.Notifications.Application.Commands.Notifications;
using FCG.Notifications.Application.Commands.Notifications.Handlers;
using FCG.Notifications.Application.Contracts;
using FCG.Notifications.Domain.Enums;
using FCG.Notifications.Domain.Exceptions;
using FCG.Notifications.Domain.ValueObjects;
using Moq;

namespace FCG.Notifications.Tests.Application.Commands.Notifications;

public class SendWelcomeEmailCommandHandlerTests
{
    [Fact(DisplayName = "Validando envio de e-mail de boas-vindas")]
    [Trait("Categoria", "Application - Notifications")]
    public async Task SendWelcomeEmail_Success()
    {
        var emailSender = new Mock<IEmailSender>();

        var handler = new SendWelcomeEmailCommandHandler(emailSender.Object);

        var correlationId = Guid.NewGuid();

        await handler.HandleAsync(new SendWelcomeEmailCommand
        {
            UserId = Guid.NewGuid(),
            Name = "Bruno",
            Email = "bruno@email.com",
            CorrelationId = correlationId
        });

        emailSender.Verify(x => x.SendAsync(
                It.Is<EmailAddress>(e => e.Value == "bruno@email.com"),
                "Bem-vindo ao FIAP Cloud Games",
                It.Is<string>(body => body.Contains("Bruno")),
                NotificationType.Welcome,
                correlationId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando envio de boas-vindas com e-mail inválido")]
    [Trait("Categoria", "Application - Notifications")]
    public async Task SendWelcomeEmail_InvalidEmail()
    {
        var emailSender = new Mock<IEmailSender>();

        var handler = new SendWelcomeEmailCommandHandler(emailSender.Object);

        var result = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new SendWelcomeEmailCommand
            {
                UserId = Guid.NewGuid(),
                Name = "Bruno",
                Email = "email-invalido",
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