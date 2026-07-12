namespace FCG.Notifications.Application.Commands.Notifications;

public class SendWelcomeEmailCommand
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Guid CorrelationId { get; set; }
}