using System;

namespace TCGOrderManagement.Shared.Messaging
{
    /// <summary>
    /// Configuration for RabbitMQ connection settings.
    /// All values are required to be set via environment variables with no default values.
    /// </summary>
    public class RabbitMqConfig
    {
        /// <summary>
        /// The RabbitMQ host address
        /// Required environment variable: RABBITMQ_HOST
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// The RabbitMQ port
        /// Required environment variable: RABBITMQ_PORT
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The RabbitMQ username for authentication
        /// Required environment variable: RABBITMQ_USERNAME
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The RabbitMQ password for authentication
        /// Required environment variable: RABBITMQ_PASSWORD
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The RabbitMQ virtual host
        /// Required environment variable: RABBITMQ_VHOST
        /// </summary>
        public string VirtualHost { get; set; }

        /// <summary>
        /// The RabbitMQ exchange name for publishing events
        /// Required environment variable: RABBITMQ_EXCHANGE
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// Validates the configuration, ensuring all required fields are present and valid.
        /// Will throw detailed exceptions for any missing or invalid configuration.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when a required field is missing or invalid</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Host))
                throw new ArgumentException("RabbitMQ Host is required. Set the RABBITMQ_HOST environment variable.");

            if (Port <= 0)
                throw new ArgumentException("RabbitMQ Port must be a positive number. Set the RABBITMQ_PORT environment variable.");

            if (string.IsNullOrWhiteSpace(Username))
                throw new ArgumentException("RabbitMQ Username is required. Set the RABBITMQ_USERNAME environment variable.");

            if (string.IsNullOrWhiteSpace(Password))
                throw new ArgumentException("RabbitMQ Password is required. Set the RABBITMQ_PASSWORD environment variable.");

            if (string.IsNullOrWhiteSpace(VirtualHost))
                throw new ArgumentException("RabbitMQ VirtualHost is required. Set the RABBITMQ_VHOST environment variable.");

            if (string.IsNullOrWhiteSpace(ExchangeName))
                throw new ArgumentException("RabbitMQ Exchange name is required. Set the RABBITMQ_EXCHANGE environment variable.");
        }
    }
} 