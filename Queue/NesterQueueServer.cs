using RabbitMQ.Client;
using System.Text;
using Inkton.Nester.Models;

namespace Inkton.Nester.Queue
{
    public class NesterQueueServer : NesterQueue
    {
        public NesterQueueServer(NesterService service,
            bool durable = false, bool autoDelete = false)
            :base(service, durable, autoDelete)
        {
        }

        public void Send(
            string message,
            Nest nest = null,
            int cushion = -1)
        {
            Send(Encoding.UTF8.GetBytes(message), 
                nest, cushion);
        }

        public void Send(
            byte[] message,
            Nest nest = null,
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

