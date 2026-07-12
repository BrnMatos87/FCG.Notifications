using FCG.Notifications.Infrastructure.Extensions;
using FCG.Notifications.Worker.Configuration;

var builder = Host.CreateApplicationBuilder(args);

var runningInContainer =
    string.Equals(
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
        "true",
        StringComparison.OrdinalIgnoreCase);

if (builder.Environment.IsDevelopment() && !runningInContainer)
{
    builder.Configuration.AddJsonFile(
        "appsettings.Local.json",
        optional: true,
        reloadOnChange: true);
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddInfrastructure();

builder.Services.AddMassTransitConfiguration(
    builder.Configuration);

var host = builder.Build();

await host.RunAsync();