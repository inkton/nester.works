using System.Collections.Generic;
using RabbitMQ.Client;
using System.Text;
using Inkton.Nest.Model;

namespace Inkton.Nester.Queue
{
    public class NesterQueueServer : NesterQueue
    {
        private IBasicProperties _props;

        public NesterQueueServer(NesterService service,
            bool durable = false, bool autoDelete = false)
            :base(service, durable, autoDelete)
        {
            _props = _channel.CreateBasicProperties();
            _props.ContentType = "text/plain";
            _props.DeliveryMode = 1;
        }

        public bool Persist
        {
            set 
            {
                 _props.DeliveryMode = (byte)(value ? 2 : 1);
            }
        }

        public string Expiration
        {
            set 
            {
                 _props.Expiration = value;
            }
        }

        public byte Priority
        {
            set 
            {
                 _props.Priority = value;
            }
        }

        public IDictionary<string, object> Headers
        {
            set 
            {
                 _props.Headers = value;
            }
        }

        public void SetDefaults()
        {
            _props.ClearExpiration();
            _props.ClearPriority();
            _props.ClearHeaders();
        }

        public void Send(
            string message,
            Inkton.Nest.Model.Nest nest = null,
            int cushion = -1)
        {
            Send(Encoding.UTF8.GetBytes(message), 
                nest, cushion);
        }

        public void Send(
            byte[] message,
            Inkton.Nest.Model.Nest nest = null,
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
                routingKey: routingKey, basicProperties: _props, body: message);
        }
    }
}

