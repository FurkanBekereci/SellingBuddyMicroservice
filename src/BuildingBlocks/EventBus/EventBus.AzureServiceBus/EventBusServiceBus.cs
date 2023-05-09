using Azure.Core;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EventBus.Base;
using EventBus.Base.Events;
using Microsoft.Azure.Amqp.Framing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.AzureServiceBus
{
    public class EventBusServiceBus : BaseEventBus
    {

        private ServiceBusClient _serviceBusClient;
        private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;
        private ServiceBusSender _senderForTopic;

        public EventBusServiceBus(EventBusConfig eventBusConfig, IServiceProvider serviceProvider) : base(eventBusConfig, serviceProvider)
        {
            SetClient();
            _serviceBusAdministrationClient = new ServiceBusAdministrationClient(eventBusConfig.EventBusConnectionString);
        }

        private ServiceBusSender CreateOrGetSenderForTopic(string topicName)
        {
            var sender = _serviceBusClient.CreateSender(topicName);
            
            return sender;
        }

        public override void Dispose()
        {
            //CloseSender();
            //CloseClient();
            base.Dispose();
        }

        private void CloseClient()
        {

            if(!(_serviceBusClient?.IsClosed ?? true))
            {
                _serviceBusClient.DisposeAsync().GetAwaiter().GetResult();
                _serviceBusClient = null;
            }
        }

        private void CloseSender()
        {

            if (!(_senderForTopic?.IsClosed ?? true))
            {
                _senderForTopic.DisposeAsync().GetAwaiter().GetResult();
                _senderForTopic = null;
            }
        }

        private void CreateTopic(string topicName)
        {
            if (!_serviceBusAdministrationClient.TopicExistsAsync(topicName).GetAwaiter().GetResult())
                _serviceBusAdministrationClient.CreateTopicAsync(topicName).GetAwaiter().GetResult();
        }


        private ServiceBusClient GetServiceBusClient(
            string connectionString, 
            ServiceBusRetryMode retryMode = ServiceBusRetryMode.Fixed, 
            double tryTimeoutInSeconds = 60, 
            int maxRetries = 3,
            double delayInSeconds = .8
            )
        {
            return new ServiceBusClient(connectionString);
        }

        private void SetClient()
        {
            _serviceBusClient = new ServiceBusClient(EventBusConfig.EventBusConnectionString);
        }

        private void SendMessageToTopic<T>(string topicName, T @event) where T : IntegrationEvent
        {
            
            SetClient();
            CreateTopic(topicName);
            CreateSubscriptionAndRule(topicName, GetSubName(@event.GetType().Name), ProcessEventName(@event.GetType().Name));
            var sender = CreateOrGetSenderForTopic(topicName);
            ServiceBusMessage message = GetSenderMessage(@event);
            sender.SendMessageAsync(message).GetAwaiter().GetResult();

        }

        private ServiceBusMessage GetSenderMessage<T>(T @event) where T : IntegrationEvent
        {
            var dataString = JsonConvert.SerializeObject(@event);
            var dataByteArray = Encoding.UTF8.GetBytes(dataString);

            var eventName = ProcessEventName(@event.GetType().Name); //Example: converting from OrderCreatedIntegrationEvent to OrderCreated;

            ServiceBusMessage message = new ServiceBusMessage(dataByteArray);
            message.MessageId = Guid.NewGuid().ToString();
            message.Subject = eventName;

            return message;

        }


        public override void Publish(IntegrationEvent @event)
        {
            SendMessageToTopic(EventBusConfig.DefaultTopicName, @event);
        }

        public override void Subscribe<T, THandler>()
        {
            if (!SubscriptionManager.HasSubscriptionForEvent<T>())
            {
                var eventName = ProcessEventName(typeof(T).Name);
                var subName = GetSubName(typeof(T).Name);
                CreateTopic(EventBusConfig.DefaultTopicName);
                CreateSubscriptionAndRule(EventBusConfig.DefaultTopicName, subName, eventName);
                StartProcessingSubscription(EventBusConfig.DefaultTopicName, subName);
            }

            SubscriptionManager.AddSubscription<T, THandler>();
        }

        private ServiceBusProcessor GetServiceBusProcessor(string queueOrTopicName, string subscriptionName = null)
        {

            ServiceBusProcessor newServiceBusProcessor;
            if (!string.IsNullOrEmpty(subscriptionName))
            {
                newServiceBusProcessor = _serviceBusClient.CreateProcessor(queueOrTopicName, subscriptionName, new ServiceBusProcessorOptions() { MaxConcurrentCalls = 10, AutoCompleteMessages = false});
            }
            else
            {
                newServiceBusProcessor = _serviceBusClient.CreateProcessor(queueOrTopicName, new ServiceBusProcessorOptions() { MaxConcurrentCalls = 10, AutoCompleteMessages = false });
            }
            newServiceBusProcessor.ProcessMessageAsync += ServiceBusProcessor_ProcessMessageAsync;
            newServiceBusProcessor.ProcessErrorAsync += ServiceBusProcessor_ProcessErrorAsync;

            return newServiceBusProcessor;
        }

        private Task ServiceBusProcessor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            var error = arg.Exception;
            Console.WriteLine(error);

            return Task.CompletedTask;
        }

        private async Task ServiceBusProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var eventName = arg.Message.Subject;
            var msgByteArr = arg.Message.Body.ToArray();
            var msgString = Encoding.UTF8.GetString(msgByteArr);

            //OnMessageArrived.Invoke(msgString ?? "");
            if(await ProcessEvent(ProcessEventName(eventName), msgString))
            {
                await arg.CompleteMessageAsync(arg.Message);
            }

        }

        private void StartProcessingSubscription(string topicName, string subscriptionName)
        {
            var processor = GetServiceBusProcessor(topicName, subscriptionName);
            processor.StartProcessingAsync();

        }

        private void CreateSubscriptionAndRule(string topicName, string subscriptionName, string ruleName)
        {
            CreateSubscription(topicName, subscriptionName);
            //Remove default rule from subscription
            RemoveRuleFromSubcription(topicName, subscriptionName, "$Default");
            CreateRule(topicName, subscriptionName, ruleName);
        }

        private void CreateSubscription(string topicName, string subscriptionName)
        {
            if (!_serviceBusAdministrationClient.SubscriptionExistsAsync(topicName, subscriptionName).GetAwaiter().GetResult())
                _serviceBusAdministrationClient.CreateSubscriptionAsync(topicName, subscriptionName).GetAwaiter().GetResult();

        }

        private void CreateRule(string topicName, string subscriptionName, string ruleName)
        {
            if (!_serviceBusAdministrationClient.RuleExistsAsync(topicName,subscriptionName,ruleName).GetAwaiter().GetResult())
                _serviceBusAdministrationClient.CreateRuleAsync(
                    topicName, 
                    subscriptionName, 
                    new CreateRuleOptions(name: ruleName, filter : new CorrelationRuleFilter() { Subject = ruleName } )
                ).GetAwaiter().GetResult();


        }

        private void RemoveRuleFromSubcription(string topicName, string subscriptionName, string ruleName)
        {
            if(_serviceBusAdministrationClient.RuleExistsAsync(topicName, subscriptionName, ruleName).GetAwaiter().GetResult())
                _serviceBusAdministrationClient.DeleteRuleAsync(topicName, subscriptionName, ruleName).GetAwaiter().GetResult();
        }

        private void StopProcessingSubscription(string topicName, string subscriptionName)
        {
            var processor = GetServiceBusProcessor(topicName, subscriptionName);
            processor.StopProcessingAsync().GetAwaiter().GetResult();
        }

        public override void Unsubscribe<T, THandler>()
        {
            var eventName = GetSubName(typeof(T).Name);

            StopProcessingSubscription(EventBusConfig.DefaultTopicName, eventName);

            RemoveRuleFromSubcription(EventBusConfig.DefaultTopicName, eventName, eventName);
            
            SubscriptionManager.RemoveSubscription<T, THandler>();
        }

    }
}
