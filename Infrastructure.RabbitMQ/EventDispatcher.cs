using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

namespace Infrastructure.RabbitMQ
{
    public class EventDispatcher : IDispatchEvent
    {
        private readonly IBusSubscription _busSubscription;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public EventDispatcher(IBusSubscription busSubscription)
        {
            _busSubscription = busSubscription;
        }
        public void Dispatch(object e)
        {
            if(e==null)throw new ArgumentNullException(nameof(e));

            using (var channel = _busSubscription.CreateChannel())
            {
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.Headers = new Dictionary<string, object>{{"event-type", e.GetType().AssemblyQualifiedName}};

                var message = JsonConvert.SerializeObject(e);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: e.GetType().FullName,
                    routingKey: "",
                    basicProperties: properties,
                    body: body, mandatory: true);
                log.Debug($"Sent event of type {e.GetType().FullName}: {message}");
            }
        }
    }
}
