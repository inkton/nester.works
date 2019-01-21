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
using System.Text;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;

namespace Inkton.Nester.Queue
{
    public class NesterQueueRPCClient : IDisposable
    {
        private SimpleRpcClient _client;
        private string _exchange;
        private IModel _model;

        private IBasicProperties _props;

        public NesterQueueRPCClient()
        {
        }

        public NesterQueueRPCClient(string exchange,
            string routingKey, IConnection connection, 
            int timeoutMilliseconds = 5000)
        {
            _exchange = exchange;
            _model = connection.CreateModel();

            _client = new SimpleRpcClient(_model, 
                exchange, "topic", routingKey);
            _client.TimeoutMilliseconds = timeoutMilliseconds;

            SetDefaults();
        }

        public SimpleRpcClient Client
        {
            get { return _client; }
        }

        public IModel Model
        {
            get { return _model; }
        }        

        public int TimeoutMilliseconds
        {
            get { return _client.TimeoutMilliseconds; }
            set { _client.TimeoutMilliseconds = value; }
        }

        public event EventHandler TimedOut
        {
            add
            {
                _client.TimedOut += value;
            }            
            remove
            {
                _client.TimedOut -= value;
            }
        }   

        public event EventHandler Disconnected
        {
            add
            {
                _client.Disconnected += value;
            }            
            remove
            {
                _client.Disconnected -= value;
            }
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

        public virtual void SetDefaults()
        {
            _props = _model.CreateBasicProperties();
            _props.ContentType = "application/json";
            _props.Headers = new Dictionary<string, object>();
        }

        public virtual byte[] Call(string function, byte[] message)
        {
            IBasicProperties replyProp;
            return Call(function, message, out replyProp);
        }

        public virtual byte[] Call(string function, byte[] message, out IBasicProperties replyProp)
        {
            _props.Headers["Function"] = function;
            return _client.Call(_props, message, out replyProp);
        }

        public virtual void Close()
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

