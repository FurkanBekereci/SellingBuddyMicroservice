using Confluent.Kafka.Admin;
using Confluent.Kafka;
using Newtonsoft.Json;
using EventBus.Base.Events;

namespace EventBus.Kafka
{
    public class KafkaClient
    {
        private Dictionary<string, bool> _subscribedTopicList;
        private Dictionary<string, Task> _topicTaskList;
        private Dictionary<string, CancellationTokenSource> _topicTaskCancellationTokenSources;
        

        public event Func<string,string, bool> OnMessageHandled;
        public event Action<Exception> OnErrorHandled;

        private IAdminClient _kafkaAdminClient;
        private IProducer<Null, string> _kafkaProducer;
        //private IConsumer<Null, string> _kafkaConsumer;

        private readonly string _serverIp;
        private readonly string _clientName;

        public KafkaClient(string serverIp, string clientName)
        {
            _serverIp = serverIp;
            _clientName = clientName;
            _subscribedTopicList = new Dictionary<string, bool>();
            _topicTaskList = new Dictionary<string, Task>();
            _topicTaskCancellationTokenSources = new Dictionary<string, CancellationTokenSource>();
        }

        public void PublishMessage(string topicName, IntegrationEvent data)
        {
            var producer = GetKafkaProducer();

            if (!IsTopicExist(topicName)) throw new Exception($"There is no topic with name: {topicName}");

            var sentMessage = new Message<Null, string>() { Value = JsonConvert.SerializeObject(data) };

            producer.Produce(topicName, sentMessage);
            
        }

        public void StartConsuming<T>(string topicName, string consumerGroupName) where T : IntegrationEvent
        {

            if (!_subscribedTopicList.Keys.Any(c => c == topicName))
            {
                SetThisCanConsumeKafkaServer<T>(topicName, consumerGroupName);
            }

            _subscribedTopicList[topicName] = true;
            
        }

        public void StopConsuming(string topicName)
        {
            _subscribedTopicList[topicName] = false;
            //_subscribedTopicList.Remove(topicName);
            _topicTaskCancellationTokenSources[topicName].Cancel();
        }

        private async void SetThisCanConsumeKafkaServer<T>(string topicName, string consumerGroupName) where T : IntegrationEvent
        {
            
            var consumer = GetKafkaConsumer(consumerGroupName);

            _topicTaskCancellationTokenSources[topicName] = new CancellationTokenSource();
            var token = _topicTaskCancellationTokenSources[topicName].Token;
            
            await Task.Run(() =>
            {
                using (consumer)
                {
                    consumer.Subscribe(topicName);
                    _subscribedTopicList[topicName] = true;

                    while (_subscribedTopicList[topicName])
                    {

                        try
                        {
                            var result = consumer.Consume(token);

                            if (result != null && !string.IsNullOrEmpty(result?.Message?.Value))
                            {
                                if (OnMessageHandled?.Invoke(result.Topic, result.Message.Value) ?? false)
                                {
                                    consumer.Commit(result);
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            _subscribedTopicList[topicName] = false;
                        }
                        catch (Exception ex)
                        {
                            OnErrorHandled?.Invoke(ex);
                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        //token.ThrowIfCancellationRequested();
                        _topicTaskCancellationTokenSources[topicName].Dispose();
                        _topicTaskCancellationTokenSources.Remove(topicName);
                        _subscribedTopicList.Remove(topicName);
                    }
                }
            }, token);
        }

        private IConsumer<Null, string> GetKafkaConsumer(string consumerGroupName)
        {
            return new ConsumerBuilder<Null, string>(new ConsumerConfig
            {
                GroupId = consumerGroupName,
                BootstrapServers = _serverIp,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
            }).Build();
        }

        private IProducer<Null, string> GetKafkaProducer()
        {
            return _kafkaProducer ??= new ProducerBuilder<Null, string>(new ProducerConfig
            {
                BootstrapServers = _serverIp,

            }).Build();
        }

        private IAdminClient GetKafkaAdmin()
        {
            return _kafkaAdminClient ??= new AdminClientBuilder(
                new AdminClientConfig
                {
                    BootstrapServers = _serverIp,
                    ClientId = _clientName
                }
            ).Build();
        }

        public async Task CreateTopic(string topicName)
        {
            var kafkaAdmin = GetKafkaAdmin();

            if (IsTopicExist(topicName))
            {
                Console.WriteLine($"The topic with name '{topicName}' is already exist");
                return;
            }

            await kafkaAdmin.CreateTopicsAsync(new List<TopicSpecification>()
            {
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = 1,
                }
            });

        }

        private bool IsTopicExist(string topicName)
        {
            var kafkaAdmin = GetKafkaAdmin();

            var topicList = kafkaAdmin.GetMetadata(TimeSpan.FromSeconds(20)).Topics;

            var isTopicExist = topicList.Any(t => t.Topic == topicName);

            return isTopicExist;
        }

        public async Task DeleteTopic(string topicName)
        {
            if (IsTopicExist(topicName)) return;

            var kafkaAdmin = GetKafkaAdmin();

            await kafkaAdmin.DeleteTopicsAsync(new[] { topicName });
        }
    }
}