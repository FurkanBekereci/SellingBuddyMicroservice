using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMQ
{
    internal class RabbitMQPersistentConnection : IDisposable
    {
        public bool IsConnected => _connection?.IsOpen ?? false;
        private IConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        private readonly int _retryCount;
        private object _lock = new object();
        private bool _disposed;

        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
        {
            _connectionFactory = connectionFactory;
            _retryCount = retryCount;
        }

        public void Dispose()
        {
            _disposed = true;
            _connection.Dispose();
        }

        public IModel CreateModel()
        {
            return _connection.CreateModel();
        }

        public bool TryToConnect()
        {
            lock (_lock)
            {
                var policy = Policy.Handle<SocketException>()
                                .Or<BrokerUnreachableException>()
                                .WaitAndRetry(
                                    _retryCount, 
                                    retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)), 
                                    (ex, time) =>
                                    {

                                    }
                                );
                policy.Execute(() =>
                {
                    _connection = _connectionFactory.CreateConnection();
                });

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += Connection_FailureOccured;
                    _connection.CallbackException += Connection_FailureOccured;
                    _connection.ConnectionBlocked += Connection_FailureOccured;
                }

                return IsConnected;
            }
        }

        //private void Connection_ConnectionBlocked(object? sender, global::RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
        //{
        //    TryToConnect();
        //}

        //private void Connection_CallbackException(object? sender, global::RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        //{
        //    TryToConnect();
        //}

        //private void Connection_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        //{
        //    //Log connection_connectionshutdown
        //    TryToConnect();
        //}

        private void Connection_FailureOccured(object? sender, object args)
        {
            if (_disposed) return;

            TryToConnect();
        }
    }
}
