using System.Threading.Tasks;

namespace TCGOrderManagement.Shared.Messaging
{
    /// <summary>
    /// Interface for handling messages received from a message broker
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Handles an incoming message from the message broker
        /// </summary>
        /// <param name="routingKey">The routing key of the message</param>
        /// <param name="message">The message content</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task HandleMessageAsync(string routingKey, string message);
    }
} 