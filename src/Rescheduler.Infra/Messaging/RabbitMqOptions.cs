namespace Rescheduler.Infra.Messaging
{
    internal class RabbitMqOptions
    {
        public string JobsExchange { get; set; } = "jobs";
    }
}