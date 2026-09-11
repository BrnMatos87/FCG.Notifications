using FCG.Notifications.Domain.Enums;
using FCG.Notifications.Domain.ValueObjects;
using FCG.Notifications.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Notifications.Tests.Infrastructure.Services;

public class ConsoleEmailSenderTests
{
    [Fact(DisplayName = "Validando envio de e-mail simulado")]
    [Trait("Categoria", "Infrastructure - Services")]
    public async Task ConsoleEmailSender_Send_Success()
    {
        var logger = new Mock<ILogger<ConsoleEmailSender>>();
        var sender = new ConsoleEmailSender(logger.Object);

        await sender.SendAsync(
            new EmailAddress("bruno@email.com"),
            "Subject",
            "Body",
            NotificationType.Welcome,
            Guid.NewGuid());

        Assert.True(true);
    }
}
