namespace Rescheduler.Infra.Messaging;

internal class MessagingOptions
{
    /// <summary>
    /// RabbitMQ options
    /// </summary>
    public RabbitMqOptions RabbitMq { get; init; } = new();

    /// <summary>
    /// Azure Service Bus options
    /// </summary>
    public ServiceBusOptions ServiceBus { get; init; } = new();

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

internal class ServiceBusOptions
{
    /// <summary>
    /// Toggle the usage of Azure Service Bus
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Toggle the usage of partitioned queues
    /// </summary>
    public bool PartitionedQueue { get; init; }

    /// <summary>
    /// The Service bus queue that jobs are published to
    /// </summary>
    public string JobsQueue { get; init; } = "jobs";

    public string ConnectionString { get; init; } = string.Empty;
}
    
internal class SnsOptions
{
    /// <summary>
    /// Toggle the usage of Azure Service Bus
    /// </summary>
    public bool Enabled { get; init; } = false;
        
    /// <summary>
    /// Toggle the usage of fifo topic
    /// </summary>
    public bool FifoTopic { get; init; } = false;
        
    /// <summary>
    /// The ARN of the Sns topic
    /// </summary>
    public string TopicArn { get; init; } = string.Empty;
}