using System;
using RabbitMQ.Client;
using System.Text;
using System.Collections.Generic;

using Inkton.Nest.Admin;

namespace Inkton.NesterWorks.Queue
{
    public class NesterQueue : IDisposable
    {
        protected IConnection _connection;
        protected IModel _channel;
        protected string _queueName;

        public static string ExchangeName = "nest_works";

        public NesterQueue(bool durable = false, bool autoDelete = false)
        {
            ConnectionFactory factory = new ConnectionFactory();
            NesterFacade facade = new NesterFacade();
            
            factory.UserName = facade.RabbitMQ.User;
            factory.Password = facade.RabbitMQ.Password;
            factory.HostName = facade.RabbitMQ.Host;            
            factory.VirtualHost = facade.RabbitMQ.Resource;

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

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
            }
            if (_channel != null)
            {
                _channel.Dispose();
            }
        }
    }
}

