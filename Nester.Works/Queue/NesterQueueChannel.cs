using System;
using RabbitMQ.Client;
using System.Text;
using System.Collections.Generic;

namespace Inkton.Nester.Queue
{
    public class NesterQueueChannel : IDisposable
    {
        private string _exchange;
        private IModel _model;
        private bool _durable;
        private bool _autoDelete;

        private IBasicProperties _props;
        private string _defaultQueue;

        public NesterQueueChannel(string exchange, IConnection connection,
            bool durable = false, bool autoDelete = true)
        {
            _exchange = exchange;
            _model = connection.CreateModel();
            _durable = durable;
            _autoDelete = autoDelete;

            SetDefaults();
        }

        public IModel Model
        {
            get { return _model; }
        }

        public IBasicProperties Properties
        {
            get 
            { 
                return _props; 
            }
            set
            {
                _props = value;
            }
        }

        public void SetDefaults()
        {
            _props = _model.CreateBasicProperties();
            _props.ContentType = "application/json";
            _props.ReplyTo =
                    Environment.GetEnvironmentVariable("NEST_TAG") + "." + 
                    Environment.GetEnvironmentVariable("NEST_CUSHION_INDEX");
            _props.ClearExpiration();
            _props.ClearPriority();
            _props.Headers = new Dictionary<string, object>();
        }

        public string Publish(string routingKey, byte[] message,
            Type type = null, string correlationId = null)
        {
            if (type != null)
            {
                _props.Headers["Type"] = type.ToString();
            }

            if (correlationId != null)
            {
                _props.CorrelationId = correlationId;
            }

            _model.BasicPublish(exchange: _exchange, 
                routingKey: routingKey, basicProperties: _props, body: message);

            return _props.CorrelationId;
        }

        public void QueueDeclare(string name)
        {
            _defaultQueue = name;
            _model.QueueDeclare(queue: _defaultQueue,
                    durable: _durable,
                    autoDelete: _autoDelete,
                    exclusive: false,
                    arguments: null);
        }

        public void QueueBind(string routingKey)
        {
            _model.QueueBind(queue: _defaultQueue,
                exchange: _exchange, routingKey: routingKey);
        }

        public void Close()
        {
            if (_model != null)
            {
                _model.Close(200, "Goodbye");
            }
        }

        public void Dispose()
        {
            Close();
        }        
    }
 
}

