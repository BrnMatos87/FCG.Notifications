using System.Text.Json;
using FCG.BuildingBlocks.Events;
using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Commands.Notifications;
using FCG.Notifications.Functions.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Notifications.Tests.Functions;

public class UserCreatedFunctionTests
{
    [Fact(DisplayName = "Validando processamento do evento UserCreated")]
    [Trait("Categoria", "Functions - UserCreated")]
    public async Task UserCreatedFunction_Run_Success()
    {
        var handler = new Mock<ICommandHandlerVoid<SendWelcomeEmailCommand>>();
        var logger = new Mock<ILogger<UserCreatedFunction>>();
        var context = new Mock<FunctionContext>();

        context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var message = new UserCreatedEvent
        {
            UserId = Guid.NewGuid(),
            Name = "Bruno",
            Email = "bruno@email.com",
            CreatedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid()
        };

        var request = CreateRequest(message);

        var function = new UserCreatedFunction(handler.Object, logger.Object);

        var result = await function.RunAsync(request, context.Object);

        Assert.IsType<NoContentResult>(result);

        handler.Verify(x => x.HandleAsync(
                It.Is<SendWelcomeEmailCommand>(c =>
                    c.UserId == message.UserId &&
                    c.Name == message.Name &&
                    c.Email == message.Email &&
                    c.CorrelationId == message.CorrelationId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando mensagem inválida sem envelope")]
    [Trait("Categoria", "Functions - UserCreated")]
    public async Task UserCreatedFunction_InvalidEnvelope_ShouldNotSendEmail()
    {
        var handler = new Mock<ICommandHandlerVoid<SendWelcomeEmailCommand>>();
        var logger = new Mock<ILogger<UserCreatedFunction>>();
        var context = new Mock<FunctionContext>();

        context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        var function = new UserCreatedFunction(handler.Object, logger.Object);

        var result = await function.RunAsync(CreateRequest(new { }), context.Object);

        Assert.IsType<BadRequestObjectResult>(result);

        handler.Verify(x => x.HandleAsync(
                It.IsAny<SendWelcomeEmailCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static HttpRequest CreateRequest<T>(T body)
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(body));
        return context.Request;
    }
}
