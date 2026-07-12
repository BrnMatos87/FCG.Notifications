using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Contracts;
using FCG.Notifications.Domain.Enums;
using FCG.Notifications.Domain.ValueObjects;

namespace FCG.Notifications.Application.Commands.Notifications.Handlers;

public class SendPurchaseConfirmationEmailCommandHandler
    : ICommandHandlerVoid<SendPurchaseConfirmationEmailCommand>
{
    private readonly IEmailSender _emailSender;

    public SendPurchaseConfirmationEmailCommandHandler(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task HandleAsync(
        SendPurchaseConfirmationEmailCommand command,
        CancellationToken ct = default)
    {
        var email = new EmailAddress(command.UserEmail);

        var subject = "Compra confirmada - FIAP Cloud Games";

        var body =
            $"Sua compra foi aprovada. Jogo: {command.GameTitle}. " +
            $"Preço: {command.Price:C}.";

        await _emailSender.SendAsync(
            email,
            subject,
            body,
            NotificationType.PurchaseConfirmation,
            command.CorrelationId,
            ct);
    }
}