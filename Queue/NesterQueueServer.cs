using System;
using RabbitMQ.Client;
using System.Text;
using System.Collections.Generic;

using Inkton.Nest.Admin;

namespace Inkton.NesterWorks.Queue
{
    public class NesterQueueServer : NesterQueue
    {
        public NesterQueueServer(bool durable = false, bool autoDelete = false)
            :base(durable, autoDelete)
        {
        }

        public void Send(
            string message,
            Inkton.Nest.Admin.Nest nest = null,
            int cushion = -1)
        {
            Send(Encoding.UTF8.GetBytes(message), 
                nest, cushion);
        }

        public void Send(
            byte[] message,
            Inkton.Nest.Admin.Nest nest = null,
            int cushion = -1)
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

            _channel.BasicPublish(exchange: "nest_works", 
                routingKey: routingKey, basicProperties: null, body: message);
        }
    }
}

