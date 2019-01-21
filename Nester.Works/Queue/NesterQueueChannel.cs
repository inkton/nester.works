/*
    Copyright (c) 2017 Inkton.

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the "Software"),
    to deal in the Software without restriction, including without limitation
    the rights to use, copy, modify, merge, publish, distribute, sublicense,
    and/or sell copies of the Software, and to permit persons to whom the Software
    is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using RabbitMQ.Client;
using System.Text;
using System.Collections.Generic;

namespace Inkton.Nester.Queue
{
    public class NesterQueueChannel : IDisposable
    {
        private NesterQueueExchange _exchange;
        private IModel _model;
        private bool _durable;
        private bool _autoDelete;

        private IBasicProperties _props;
        private string _defaultQueue;

        public NesterQueueChannel(NesterQueueExchange exchange, IConnection connection,
            bool durable = false, bool autoDelete = true)
        {
            _exchange = exchange;                                                
            _durable = durable;
            _autoDelete = autoDelete;

            _model = connection.CreateModel();
            _model.ExchangeDeclare(exchange: _exchange.Name,
                                    type: "topic");

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
                    _exchange.Name + "." +
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

            _model.BasicPublish(exchange: _exchange.Name, 
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
                exchange: _exchange.Name, routingKey: routingKey);
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

