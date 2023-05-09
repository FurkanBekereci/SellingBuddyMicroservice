using EventBus.Base;
using EventBus.Base.Events;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : BaseEventBus
    {
        private readonly RabbitMQPersistentConnection _persistentConnection;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IModel _consumerChannel;

        public EventBusRabbitMQ(EventBusConfig eventBusConfig, IServiceProvider serviceProvider) : base(eventBusConfig, serviceProvider)
        {
            if(eventBusConfig.Connection != null)
            {
                var connJson = JsonConvert.SerializeObject(eventBusConfig.Connection, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                _connectionFactory = JsonConvert.DeserializeObject<ConnectionFactory>(connJson)!;
            }
            else
            {
                _connectionFactory = new ConnectionFactory();
            }

            _persistentConnection = new RabbitMQPersistentConnection(_connectionFactory, eventBusConfig.ConnectionRetryCount);
            _consumerChannel = CreateConsumerChannel();
        }

        public override void Publish(IntegrationEvent @event)
        {
            var policy = Policy.Handle<BrokerUnreachableException>().Or<SocketException>()
                .WaitAndRetry(EventBusConfig.ConnectionRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)), (_,_) => { /*log*/ });

            var eventName = ProcessEventName(@event.GetType().Name);
            _consumerChannel.ExchangeDeclare(EventBusConfig.DefaultTopicName, "direct");

            var messageString = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(messageString);

            policy.Execute(() =>
            {
                var props = _consumerChannel.CreateBasicProperties();
                props.DeliveryMode = 2; //Persistent;
                //var subName = GetSubName(@event.GetType().Name);
                //_consumerChannel.QueueDeclare(subName, true, false, false, null);
                //_consumerChannel.QueueBind(subName, EventBusConfig.DefaultTopicName, eventName);
                _consumerChannel.BasicPublish(EventBusConfig.DefaultTopicName,eventName, mandatory:true, basicProperties: props, body: body);
            });

        }

        public override void Subscribe<T, THandler>()
        {
            var eventName = ProcessEventName(typeof(T).Name);
            var subName = GetSubName(typeof(T).Name);
            if (!SubscriptionManager.HasSubscriptionForEvent<T>())
            {
                CreateConnection();

                _consumerChannel.QueueDeclare(subName, true, false, false, null);
                _consumerChannel.QueueBind(subName, EventBusConfig.DefaultTopicName, eventName);
            }

            SubscriptionManager.AddSubscription<T, THandler>();
            StartBasicConsume(eventName);
            SubscribeToRemovingEvent();
        }

        private void SubscribeToRemovingEvent()
        {
            SubscriptionManager.OnEventRemoved += SubscriptionManager_OnEventRemoved;
        }

        private void SubscriptionManager_OnEventRemoved(object? sender, string e)
        {
            var eventName = ProcessEventName(e);

            CreateConnection();
            _consumerChannel.QueueUnbind(eventName, EventBusConfig.DefaultTopicName, eventName);

            if (SubscriptionManager.IsEmpty)
            {
                _consumerChannel.Close();
            }
        }

        private void CreateConnection()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryToConnect();
            }
        }

        private void StartBasicConsume(string eventName)
        {
            var consumer = new EventingBasicConsumer(_consumerChannel);

            consumer.Received += Consumer_Received;

            _consumerChannel.BasicConsume(GetSubName(eventName), false, consumer);
        }

        private async void Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            var eventName = ProcessEventName(e.RoutingKey);

            var message = Encoding.UTF8.GetString(e.Body.Span);

            try
            {
                await ProcessEvent(eventName, message);
                _consumerChannel.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception)
            {
                //Logging...
            }
        }

        public override void Unsubscribe<T, THandler>()
        {
            SubscriptionManager.RemoveSubscription<T, THandler>();
        }

        public IModel CreateConsumerChannel()
        {
            CreateConnection();

            var channel = _persistentConnection.CreateModel();
            channel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName, type : "direct");

            return channel;
        }
    }
}
