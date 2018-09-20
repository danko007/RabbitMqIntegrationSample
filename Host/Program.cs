using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac;
using Infrastructure;
using Infrastructure.RabbitMQ;
using Newtonsoft.Json;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Host
{
    class Program
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public static void Main(string[] args)
        {
            var builder = new ContainerBuilder();

            string[] scannerPattern = {@".*BoundedContext\.dll", @"Infrastructure.RabbitMQ\.dll"};

            var assemblies = new List<Assembly>();

            assemblies.AddRange(Directory
                .EnumerateFiles(Directory.GetCurrentDirectory(), "*.dll", SearchOption.AllDirectories)
                .Where(filename => scannerPattern.Any(
                    pattern => Regex.IsMatch(filename, pattern, RegexOptions.IgnoreCase))).Select(Assembly.LoadFrom));

            builder.RegisterType<BusSubscription>().As<IBusSubscription>().InstancePerLifetimeScope();
            
            builder.RegisterAssemblyTypes(assemblies.ToArray()).AsImplementedInterfaces();
            
            var container = builder.Build();

            using (var lts = container.BeginLifetimeScope())
            {
                IEnumerable commandHandlers = lts.Resolve<IEnumerable<IHandleCommand>>();
                IEnumerable eventDeclarations = lts.Resolve<IEnumerable<IDeclareEvent>>();
                IEnumerable eventHandlers = lts.Resolve<IEnumerable<IHandleEvent>>();

                using (IBusSubscription sub = lts.Resolve<IBusSubscription>())
                using (RegisterCommandHandlers(sub, commandHandlers))
                using (RegisterEvents(sub.CreateChannel(), eventDeclarations))
                using (RegisterEventHandlers(sub, eventHandlers))
                {
                    log.Info("All services up");
                    Console.ReadLine();
                }

            }
        }

        private static IDisposable RegisterEventHandlers(IBusSubscription sub, IEnumerable eventHandlers)
        {
            var lst = new List<IDisposable>();
            foreach (var eHandler in eventHandlers)
            {
                var interfaces = eHandler.GetType().GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandleEvent<>))
                    .ToArray();

                var channel = sub.CreateChannel();
                var queueName = eHandler.GetType().FullName;

                foreach (var impl in interfaces)
                {
                    RegisterEventHandler(queueName, impl.GetGenericArguments()[0], channel);
                }
                
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body;
                    try
                    {
                        var props = ea.BasicProperties;
                        var eventTypeNameBytes = props.Headers["event-type"];
                        var eventType = Type.GetType(Encoding.UTF8.GetString((Byte[])eventTypeNameBytes), true);

                        var message = Encoding.UTF8.GetString(body);
                        

                        var cmd = JsonConvert.DeserializeObject(message, eventType);
                        var handlerMethod = GetHandlerMethod(eHandler, eventType, "HandleAsync");
                        try
                        {
                            log.Debug($"Got event of type {eventType.FullName}: {message}");
                            await (Task) handlerMethod.Invoke(eHandler, new[] {cmd});
                            log.Debug($"Successfully processed event {eventType.FullName}:  {message}");
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, $"Error occurred in processing of event { eventType.FullName }:  {message}");
                            //TODO: event is to be placed into dead letters table
                        }
                    }
                    catch (Exception e)
                    {
                        log.Fatal(e, $"Error occurred wile preparing obtained event to handling. {body}");
                        //TODO: event is to be placed into dead letters table
                    }
                    finally
                    {
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                };
                channel.BasicConsume(queue: queueName,
                    autoAck: false,
                    consumer: consumer);
                lst.Add(channel);
            }
            return new ListOfDisposables(lst.ToArray());
        }

        private static void RegisterEventHandler(string queueName, Type eventType, IModel channel)
        {
            channel.QueueDeclare(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.QueueBind(queue: queueName,
                exchange: eventType.FullName,
                routingKey: "");
        }

        private static IModel RegisterEvents(IModel channel, IEnumerable eventDeclarations)
        {

            foreach (var eDeclaration in eventDeclarations)
            {
                var interfaces = eDeclaration.GetType().GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDeclareEvent<>))
                    .ToArray();

                foreach (var impl in interfaces)
                {
                    RegisterEvent(impl.GetGenericArguments()[0], channel);
                }
            }
            return channel;
        }

        private static void RegisterEvent(Type eventType, IModel channel)
        {
            log.Debug($"Event type {eventType.FullName} is to be registered");
            channel.ExchangeDeclare(exchange: eventType.FullName, type: "topic");
            log.Debug($"Event type {eventType.FullName} is registered");
        }

        private static IDisposable RegisterCommandHandlers(IBusSubscription subscription, IEnumerable commandHandlers)
        {
            var lst = new List<IDisposable>();
            foreach (var cHandler in commandHandlers)
            {
                var interfaces = cHandler.GetType().GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandleCommand<>))
                    .ToArray();

                foreach (var impl in interfaces)
                {
                    lst.Add(RegisterCommandHandler(cHandler, impl, subscription));
                }
            }
            return new ListOfDisposables(lst.ToArray());
        }

        private static IDisposable RegisterCommandHandler(object cHandler, Type impl, IBusSubscription subscription)
        {
            var channel = subscription.CreateChannel();

            var handlerCmdType = impl.GetGenericArguments()[0];

            var consumer = ConfigureCommandQueue(channel, handlerCmdType.FullName);

            log.Debug($"Awaiting {handlerCmdType.FullName} requests");

            var handlerMethod = GetHandlerMethod(cHandler, handlerCmdType, "HandleAsync");

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body;

                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;
                log.Trace($"Got command with correlationid={props.CorrelationId}");
                string responseSerialized = "";
                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    log.Debug($"Got command {message} of type {handlerCmdType.FullName}, correlationId: {props.CorrelationId}");
                    var cmd = JsonConvert.DeserializeObject(message, handlerCmdType);

                    await (Task)handlerMethod.Invoke(cHandler, new[] {cmd});
                    log.Debug($"Successfully processed command: {message}, correlationId: {props.CorrelationId}");
                }
                catch (Exception e)
                {
                    log.Error( e,
                        $"Error occurred in processing of command with correlationId: {props.CorrelationId}");
                    responseSerialized =
                        JsonConvert.SerializeObject(new CommandFailed {Code = "", Message = e.Message});
                }
                finally
                {
                    channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps,
                        body: Encoding.UTF8.GetBytes(responseSerialized));
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            return channel;
        }

        private static EventingBasicConsumer ConfigureCommandQueue(IModel channel, string queueName)
        {

            channel.QueueDeclare(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            return consumer;
        }

        private static MethodInfo GetHandlerMethod(object cHandler, Type handlerCmdType, string methodName)
        {
            var methods = cHandler.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.Name == methodName && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == handlerCmdType).ToArray();

            if (methods.Length > 1)
                throw new Exception($"More than one handler for {handlerCmdType.FullName} is found");

            if (!methods.Any())
                throw new Exception($"No handler for {handlerCmdType.FullName} is found");
            return methods[0];
        }
    }
}
