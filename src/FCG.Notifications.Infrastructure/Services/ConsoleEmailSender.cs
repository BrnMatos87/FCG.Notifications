using FCG.Notifications.Application.Contracts;
using FCG.Notifications.Domain.Enums;
using FCG.Notifications.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FCG.Notifications.Infrastructure.Services;

public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(
        EmailAddress to,
        string subject,
        string body,
        NotificationType notificationType,
        Guid correlationId,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "E-mail simulado processado. Tipo: {NotificationType}, CorrelationId: {CorrelationId}",
            notificationType,
            correlationId);

        return Task.CompletedTask;
    }
}
