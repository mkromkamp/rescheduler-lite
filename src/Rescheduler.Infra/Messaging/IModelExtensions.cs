using System.Collections.Generic;
using RabbitMQ.Client;

namespace Rescheduler.Infra.Messaging
{
    internal static class IModelExtensions
    {
        /// <summary>
        /// Ensure the given topic and queue are create and have a route configured between them
        /// </summary>
        /// <param name="model">The <see cref="IModel" /></param>
        /// <param name="topicName">The name of the topic</param>
        /// <param name="queueName">The queue of the queue</param>
        public static void EnsureConfig(this IModel model, string topicName, string queueName)
        {
            model.EnsureTopic(topicName);
            model.EnsureQueue(queueName);

            model.EnsureRoute(topicName, queueName, queueName);
        }

        /// <summary>
        /// Ensure a topic is declared.
        /// </summary>
        /// <param name="model">The <see cref="IModel"/></param>
        /// <param name="topicName">The name of the topic</param>
        public static void EnsureTopic(this IModel model, string topicName)
        {
            model.ExchangeDeclare(topicName, ExchangeType.Topic, true);
        }

        /// <summary>
        /// Ensure a queue is declared.
        /// </summary>
        /// <param name="model">The <see cref="IModel"/></param>
        /// <param name="queueName">The name of the queue</param>
        public static void EnsureQueue(this IModel model, string queueName)
        {
            var props = new Dictionary<string, object> { {"x-queue-type", "quorum"} };
            model.QueueDeclare(queueName, true, false, false, props);
        }

        /// <summary>
        /// Ensure the given queue has a route bind to the given exchange with the given routing key.
        /// </summary>
        /// <param name="model">The <see cref="IModel"/></param>
        /// <param name="topicName">The name of the topic</param>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="routingKey">The routing key</param>
        public static void EnsureRoute(this IModel model, string topicName, string queueName, string routingKey)
        {
            model.QueueBind(queueName, topicName, routingKey);
        }
    }
}