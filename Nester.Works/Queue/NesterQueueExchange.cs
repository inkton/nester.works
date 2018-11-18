using System;
using RabbitMQ.Client;
using System.Text;
using System.Collections.Generic;

namespace Inkton.Nester.Queue
{
    public class NesterQueueExchange : IDisposable
    {
        public readonly string ExchangeName = "amq.topic";
        protected IConnection _connection;
        protected NesterQueueChannel _channel;

        protected bool _durable;
        protected bool _autodelete;
        protected string _lastCorrelationId;

        public NesterQueueExchange(NesterService service, 
            bool durable = false, bool autoDelete = true)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = service.User;
            factory.Password = service.Password;
            factory.HostName = service.Host;
            factory.VirtualHost = service.Resource;
            factory.ContinuationTimeout = TimeSpan.FromSeconds(service.TimeoutSec);

            _connection = factory.CreateConnection();

            _durable = durable;
            _autodelete = autoDelete;

            _channel = CreateChannel();
        }

        public NesterQueueChannel CreateChannel()
        {
            return new NesterQueueChannel(
                ExchangeName, _connection, 
                _durable, _autodelete);
        }

        public IConnection Connection
        {
            get { return _connection; }
        }

        public NesterQueueChannel DefaultChannel
        {
            get { return _channel; }
        }

        public string LastCorrelationId
        {
            get { return _lastCorrelationId; }
            set { _lastCorrelationId = value; }
        }

        public void Close()
        {
            if (_connection != null)
            {
                _channel.Close();
                _connection.Close();
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}

