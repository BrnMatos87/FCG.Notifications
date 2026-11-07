using System.Text.Json;
using FCG.BuildingBlocks.Events;
using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Commands.Notifications;
using FCG.Notifications.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FCG.Notifications.Functions.Functions;

public class UserCreatedFunction
{
    private readonly ICommandHandlerVoid<SendWelcomeEmailCommand> _handler;
    private readonly ILogger<UserCreatedFunction> _logger;

    public UserCreatedFunction(
        ICommandHandlerVoid<SendWelcomeEmailCommand> handler,
        ILogger<UserCreatedFunction> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [Function("UserCreatedFunction")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "post",
            Route = "notifications/user-created")]
        HttpRequest request,
        FunctionContext context)
    {
        UserCreatedEvent? userCreatedEvent;

        try
        {
            userCreatedEvent = await request.ReadFromJsonAsync<UserCreatedEvent>(
                context.CancellationToken);
        }
        catch (Exception exception) when (
            exception is BadHttpRequestException or JsonException)
        {
            _logger.LogWarning(exception, "Payload de notificação de usuário inválido.");
            return new BadRequestObjectResult("O corpo da requisição contém JSON inválido.");
        }

        if (userCreatedEvent is null ||
            userCreatedEvent.UserId == Guid.Empty ||
            string.IsNullOrWhiteSpace(userCreatedEvent.Name) ||
            string.IsNullOrWhiteSpace(userCreatedEvent.Email) ||
            userCreatedEvent.CorrelationId == Guid.Empty)
        {
            _logger.LogWarning(
                "Payload de notificação de usuário não contém todos os campos obrigatórios.");

            return new BadRequestObjectResult(
                "UserId, Name, Email e CorrelationId são obrigatórios.");
        }

        _logger.LogInformation(
            "Notificação de usuário recebida. UserId: {UserId}, CorrelationId: {CorrelationId}",
            userCreatedEvent.UserId,
            userCreatedEvent.CorrelationId);

        try
        {
            await _handler.HandleAsync(
                new SendWelcomeEmailCommand
                {
                    UserId = userCreatedEvent.UserId,
                    Name = userCreatedEvent.Name,
                    Email = userCreatedEvent.Email,
                    CorrelationId = userCreatedEvent.CorrelationId
                },
                context.CancellationToken);
        }
        catch (DomainException exception)
        {
            _logger.LogWarning(
                "Notificação de usuário rejeitada por dados inválidos. UserId: {UserId}, CorrelationId: {CorrelationId}",
                userCreatedEvent.UserId,
                userCreatedEvent.CorrelationId);

            return new BadRequestObjectResult(exception.Message);
        }

        return new NoContentResult();
    }
}
