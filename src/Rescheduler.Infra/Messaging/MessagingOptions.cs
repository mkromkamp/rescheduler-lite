namespace Rescheduler.Infra.Messaging
{
    internal class MessagingOptions
    {
        /// <summary>
        /// RabbitMQ options
        /// </summary>
        public RabbitMqOptions RabbitMq { get; set; } = new RabbitMqOptions();

        public ServiceBusOptions ServiceBus { get; set; } = new ServiceBusOptions();
    }
    
    internal class RabbitMqOptions
    {
        /// <summary>
        /// Toggle the usage of RabbitMQ
        /// </summary>
        public bool Enabled { get; set; } = false;
        
        /// <summary>
        /// The name of the exchange that jobs are published to
        /// </summary>
        public string JobsExchange { get; set; } = "jobs";

        /// <summary>
        /// RabbitMQ connection string. Should be able to publish messages, create topics, queues, and bindings.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
    }

    internal class ServiceBusOptions
    {
        /// <summary>
        /// Toggle the usage of Azure Service Bus
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Toggle the usage of partitioned queues
        /// </summary>
        public bool PartitionedQueue { get; set; } = false;

        /// <summary>
        /// The Service bus queue that jobs are published to
        /// </summary>
        public string JobsQueue { get; set; } = "jobs";

        public string ConnectionString { get; set; } = string.Empty;
    }
}