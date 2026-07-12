using FCG.Notifications.Application.Abstractions.Commands;
using FCG.Notifications.Application.Commands.Notifications;
using FCG.Notifications.Application.Commands.Notifications.Handlers;
using FCG.Notifications.Application.Contracts;
using FCG.Notifications.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Notifications.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, ConsoleEmailSender>();

        services.AddScoped<ICommandHandlerVoid<SendWelcomeEmailCommand>, SendWelcomeEmailCommandHandler>();

        services.AddScoped<
            ICommandHandlerVoid<SendPurchaseConfirmationEmailCommand>,
            SendPurchaseConfirmationEmailCommandHandler>();

        return services;
    }
}