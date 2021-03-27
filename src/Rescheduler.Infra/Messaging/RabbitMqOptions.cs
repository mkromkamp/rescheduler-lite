namespace Rescheduler.Infra.Messaging
{
    internal class RabbitMqOptions
    {
        /// <summary>
        /// The name of the exchange that jobs are published to
        /// </summary>
        public string JobsExchange { get; set; } = "jobs";
    }
}