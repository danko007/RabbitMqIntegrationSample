using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.RabbitMQ
{
    public class CommandDispatcher : IDispatchCommand, IDisposable
    {
        private readonly IModel channel;

        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<object>> respsMap = new ConcurrentDictionary<Guid, TaskCompletionSource<object>>();


        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public CommandDispatcher(IBusSubscription busSubscription)
        {
            channel = busSubscription.CreateChannel();

            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);
            
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);
               
                TaskCompletionSource<object> tcs;
                if (respsMap.TryRemove(Guid.Parse(ea.BasicProperties.CorrelationId), out tcs))
                {
                    if (response == "")
                    {
                        tcs.SetResult(null);
                        log.Debug($"Got the success response correlationId: {ea.BasicProperties.CorrelationId}");
                    }
                    else
                    {
                        var error = JsonConvert.DeserializeObject<CommandFailed>(response);
                        log.Debug($"Got the failure response correlationId: {ea.BasicProperties.CorrelationId}, message: {error.Message}");
                        tcs.SetException(new CommandHandlingException(error.Message));
                    }
                }
                else
                {
                    log.Warn($"Didn't manage to find correlationId: {ea.BasicProperties.CorrelationId} in the map");
                }
            };
        }

        public Task DispatchAsync(object message)
        {
            if(message==null)
                throw new ArgumentNullException(nameof(message));

            var messageSerialized = JsonConvert.SerializeObject(message);

            var messageBytes = Encoding.UTF8.GetBytes(messageSerialized);
            
            var props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid();
            props.CorrelationId = correlationId.ToString();
            props.ReplyTo = replyQueueName;

            var tcs = new TaskCompletionSource<object>();
            if (respsMap.TryAdd(correlationId, tcs))
            {

                channel.BasicPublish(
                    exchange: "",
                    routingKey: message.GetType().FullName,
                    basicProperties: props,
                    body: messageBytes);

                channel.BasicConsume(
                    consumer: consumer,
                    queue: replyQueueName,
                    autoAck: true);

                return tcs.Task;
            }
            else
            {
                var mes = $"The slot for correlationId {correlationId} has already been engaged!";
                log.Warn(mes);
                throw new Exception(mes);
            }
        }

        public void Dispose()
        {
            channel?.Dispose();
        }

    }
}
