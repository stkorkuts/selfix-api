namespace Selfix.Shared.Settings;

public sealed class EventStreamingSettings
{
    public const string KEY = "EventStreaming";
    public required string KafkaBootstrapServers { get; init; }
    public required string ConsumerGroupId { get; init; }
    public required string CreateAvatarRequestsTopicName { get; init; }
    public required string CreateAvatarResponsesTopicName { get; init; }
    public required string ProcessPromptRequestsTopicName { get; init; }
    public required string ProcessPromptResponsesTopicName { get; init; }
    public required string GenerateImageRequestsTopicName { get; init; }
    public required string GenerateImageResponsesTopicName { get; init; }
    public required string KafkaUsername { get; set; }
    public required string KafkaPassword { get; set; }
    public required string KafkaSaslMechanism { get; set; }
    public required string KafkaSecurityProtocol { get; set; }
}