using System;
using System.Threading.Tasks;

namespace TCGOrderManagement.Shared.Messaging
{
    /// <summary>
    /// Interface for message bus implementations
    /// </summary>
    public interface IMessageBus : IDisposable
    {
        /// <summary>
        /// Publishes a message to the message bus
        /// </summary>
        /// <typeparam name="T">Type of message</typeparam>
        /// <param name="message">Message to publish</param>
        /// <param name="routingKey">Routing key for the message</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task PublishAsync<T>(T message, string routingKey);

        /// <summary>
        /// Subscribes to messages of a specific type
        /// </summary>
        /// <typeparam name="T">Type of message</typeparam>
        /// <param name="handler">Handler function for the message</param>
        /// <param name="queueName">Queue name to subscribe to</param>
        /// <param name="routingKey">Routing key to filter messages</param>
        void Subscribe<T>(Func<T, Task> handler, string queueName, string routingKey);
    }
} 