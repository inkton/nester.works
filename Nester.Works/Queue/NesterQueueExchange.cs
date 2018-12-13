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

