namespace Selfix.Schema.Kafka.Notifications.V1;

public sealed class ShowTrialPackageEvent
{
    public required long TelegramProfileId { get; init; }
}