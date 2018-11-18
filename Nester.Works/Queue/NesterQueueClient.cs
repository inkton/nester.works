using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;
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

        public SimpleRpcClient CreateRPCEndpoint(string queue)
        {
            return new SimpleRpcClient(
                DefaultChannel.Model, queue);
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
                routingKey = nest.Tag;

                if (cushion > 0)
                {
                    routingKey += "." + cushion.ToString();
                }
                else
                {
                    routingKey += ".*";
                }
            }
            
            _lastCorrelationId = DefaultChannel
                .Publish(routingKey, message, type, correlationId);
        }
    }
}

