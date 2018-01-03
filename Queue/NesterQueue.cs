using System;
using RabbitMQ.Client;
using System.Text;
using System.Collections.Generic;
using Inkton.Nester;

namespace Inkton.Nester.Queue
{
    public class NesterQueue : IDisposable
    {
        protected IConnection _connection;
        protected IModel _channel;
        protected string _queueName;

        public static string ExchangeName = "nest_works";

        public NesterQueue(NesterService service, 
            bool durable = false, bool autoDelete = false)
        {
            ConnectionFactory factory = new ConnectionFactory();

            factory.UserName = service.User;
            factory.Password = service.Password;
            factory.HostName = service.Host;            
            factory.VirtualHost = service.Resource;

            _connection = factory.CreateConnection();
 
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: ExchangeName, type: "topic", 
                durable: durable, autoDelete : autoDelete);
            _queueName = _channel.QueueDeclare().QueueName;
        }

        public IConnection Connnection
        {
            get { return _connection; }
        }

        public IModel Channel
        {
            get { return _channel; } 
        }

        public string QueueName
        {
            get { return _queueName; }
        }

        public void Close()
        {
            if (_channel != null)
            {
                _channel.Close(200, "Goodbye");
                _channel.Dispose();
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}

