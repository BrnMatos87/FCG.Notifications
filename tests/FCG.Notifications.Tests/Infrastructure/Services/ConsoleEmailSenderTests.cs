using FCG.Notifications.Domain.Enums;
using FCG.Notifications.Domain.ValueObjects;
using FCG.Notifications.Infrastructure.Services;

namespace FCG.Notifications.Tests.Infrastructure.Services;

public class ConsoleEmailSenderTests
{
    [Fact(DisplayName = "Validando envio de e-mail no console")]
    [Trait("Categoria", "Infrastructure - Services")]
    public async Task ConsoleEmailSender_Send_Success()
    {
        var sender = new ConsoleEmailSender();

        await sender.SendAsync(
            new EmailAddress("bruno@email.com"),
            "Subject",
            "Body",
            NotificationType.Welcome,
            Guid.NewGuid());

        Assert.True(true);
    }
}