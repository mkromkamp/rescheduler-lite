namespace Rescheduler.Infra.Messaging;

internal class MessagingOptions
{
    /// <summary>
    /// RabbitMQ options
    /// </summary>
    public RabbitMqOptions RabbitMq { get; init; } = new();

    /// <summary>
    /// Aws Sns options
    /// </summary>
    public SnsOptions Sns { get; init; } = new();
}
    
internal class RabbitMqOptions
{
    /// <summary>
    /// Toggle the usage of RabbitMQ
    /// </summary>
    public bool Enabled { get; init; }
        
    /// <summary>
    /// The name of the exchange that jobs are published to
    /// </summary>
    public string JobsExchange { get; init; } = "jobs";

    /// <summary>
    /// RabbitMQ connection string. Should be able to publish messages, create topics, queues, and bindings.
    /// </summary>
    public string ConnectionString { get; init; } = string.Empty;
}
    
internal class SnsOptions
{
    /// <summary>
    /// Toggle the usage of AWS SNS
    /// </summary>
    public bool Enabled { get; init; }
        
    /// <summary>
    /// Toggle the usage of fifo topic
    /// </summary>
    public bool FifoTopic { get; init; }
        
    /// <summary>
    /// The ARN of the Sns topic
    /// </summary>
    public string TopicArn { get; init; } = string.Empty;
}