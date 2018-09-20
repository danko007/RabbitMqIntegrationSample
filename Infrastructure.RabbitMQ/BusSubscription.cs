using System;
using RabbitMQ.Client;

namespace Infrastructure.RabbitMQ
{
    public interface IBusSubscription : IDisposable
    {
        IModel CreateChannel();
    }
    public class BusSubscription : IBusSubscription
    {
        private readonly IConnection connection;
        public BusSubscription()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
        }

        public void Dispose()
        {
            connection?.Dispose();
        }

        public IModel CreateChannel()
        {
            return connection.CreateModel();
        }
    }
}