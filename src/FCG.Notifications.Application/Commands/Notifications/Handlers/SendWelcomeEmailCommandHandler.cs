using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Contracts;
using FCG.Notifications.Domain.Enums;
using FCG.Notifications.Domain.ValueObjects;

namespace FCG.Notifications.Application.Commands.Notifications.Handlers;

public class SendWelcomeEmailCommandHandler : ICommandHandlerVoid<SendWelcomeEmailCommand>
{
    private readonly IEmailSender _emailSender;

    public SendWelcomeEmailCommandHandler(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task HandleAsync(
        SendWelcomeEmailCommand command,
        CancellationToken ct = default)
    {
        var email = new EmailAddress(command.Email);

        var subject = "Bem-vindo ao FIAP Cloud Games";

        var body =
            $"Olá {command.Name}, bem-vindo(a) ao FIAP Cloud Games." +
            "Sua conta foi criada com sucesso.";

        await _emailSender.SendAsync(
            email,
            subject,
            body,
            NotificationType.Welcome,
            command.CorrelationId,
            ct);
    }
}