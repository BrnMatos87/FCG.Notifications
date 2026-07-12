using FCG.Notifications.Domain.Enums;
using FCG.Notifications.Domain.ValueObjects;

namespace FCG.Notifications.Application.Contracts;

public interface IEmailSender
{
    Task SendAsync(
        EmailAddress to,
        string subject,
        string body,
        NotificationType notificationType,
        Guid correlationId,
        CancellationToken ct = default);
}