namespace FCG.Notifications.Application.Commands.Notifications;

public class SendPurchaseConfirmationEmailCommand
{
    public Guid OrderId { get; set; }

    public Guid UserId { get; set; }

    public Guid GameId { get; set; }

    public string UserEmail { get; set; } = string.Empty;

    public string GameTitle { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public Guid CorrelationId { get; set; }
}