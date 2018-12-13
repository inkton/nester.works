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
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Inkton.Nest.Model;

namespace Inkton.Nester.Queue
{
    public class NesterQueueClient : NesterQueueExchange
    {
        public NesterQueueClient(NesterService service,
            bool durable = false, bool autoDelete = true)
            :base(service, durable, autoDelete)
        {
        }

        private string GetQueue(
            Inkton.Nest.Model.Nest nest, int cushion = -1)
        {
            string routingKey = nest.Tag;

            if (cushion > 0)
            {
                routingKey += "." + cushion.ToString();
            }
            else
            {
                routingKey += ".*";
            }

            return routingKey;
        }

        public NesterQueueRPCClient CreateRPCEndpoint(
            Inkton.Nest.Model.Nest nest, int cushion = -1)
        {
            return new NesterQueueRPCClient(ExchangeName, 
                GetQueue(nest, cushion), _connection);
        }

        public void Send<T>(T message, Inkton.Nest.Model.Nest nest,        
            Type type = null, string correlationId = null, int cushion = -1)
        {
            string payload = JsonConvert.SerializeObject(message);
            Send(Encoding.UTF8.GetBytes(payload), nest, 
                type, correlationId, cushion);
        }

        public void Send(byte[] message, Inkton.Nest.Model.Nest nest,        
            Type type = null, string correlationId = null, int cushion = -1)
        {
            string routingKey = "#";
            if (nest != null)
            {
                routingKey = GetQueue(nest, cushion);
            }
            
            _lastCorrelationId = DefaultChannel
                .Publish(routingKey, message, type, correlationId);
        }
    }
}

