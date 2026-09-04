using System.Text.Json;

namespace FCG.Notifications.Functions.Messaging;

public class MassTransitEnvelope<T>
{
    public T? Message { get; set; }
}