using FCG.Notifications.Infrastructure.Messaging;
using FCG.Notifications.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Options;

namespace FCG.Notifications.Worker.Configuration;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddMassTransitConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Host),
                "RabbitMq:Host não foi configurado.")
            .Validate(
                options => options.Port > 0,
                "RabbitMq:Port deve ser maior que zero.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.VirtualHost),
                "RabbitMq:VirtualHost não foi configurado.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Username),
                "RabbitMq:Username não foi configurado.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Password),
                "RabbitMq:Password não foi configurado.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.UserCreatedQueue),
                "RabbitMq:UserCreatedQueue não foi configurado.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.PaymentProcessedQueue),
                "RabbitMq:PaymentProcessedQueue não foi configurado.")
            .ValidateOnStart();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserCreatedConsumer>();
            x.AddConsumer<PaymentProcessedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var options = context
                    .GetRequiredService<IOptions<RabbitMqOptions>>()
                    .Value;

                cfg.Host(
                    options.Host,
                    options.Port,
                    options.VirtualHost,
                    hostConfiguration =>
                    {
                        hostConfiguration.Username(options.Username);
                        hostConfiguration.Password(options.Password);
                    });

                cfg.ReceiveEndpoint(
                    options.UserCreatedQueue,
                    endpoint =>
                    {
                        endpoint.ConfigureConsumer<UserCreatedConsumer>(
                            context);
                    });

                cfg.ReceiveEndpoint(
                    options.PaymentProcessedQueue,
                    endpoint =>
                    {
                        endpoint.ConfigureConsumer<PaymentProcessedConsumer>(
                            context);
                    });
            });
        });

        return services;
    }
}