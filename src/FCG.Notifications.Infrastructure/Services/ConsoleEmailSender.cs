using FCG.Notifications.Application.Contracts;
using FCG.Notifications.Domain.Enums;
using FCG.Notifications.Domain.ValueObjects;

namespace FCG.Notifications.Infrastructure.Services;

public class ConsoleEmailSender : IEmailSender
{
    public Task SendAsync(
        EmailAddress to,
        string subject,
        string body,
        NotificationType notificationType,
        Guid correlationId,
        CancellationToken ct = default)
    {
        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("****************************************************");
        Console.WriteLine("Notificação");
        Console.WriteLine("-------------------------------------------");
        Console.WriteLine($"Tipo..........: {notificationType}");
        Console.WriteLine($"Para..........: {to}");
        Console.WriteLine($"Assunto.......: {subject}");
        Console.WriteLine($"CorrelationId.: {correlationId}");
        Console.WriteLine();
        Console.WriteLine(body);
        Console.WriteLine("****************************************************");

        Console.ResetColor();

        return Task.CompletedTask;
    }
}