using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Inkton.Nester.Queue
{    
    public class NesterQueueClient : NesterQueue
    {        
        public NesterQueueClient(NesterService service,
            bool wantCushionMsgs = false, bool durable = false, 
            bool autoDelete = false)
            :base(service, durable, autoDelete)
        {
            string routingKey = Environment.GetEnvironmentVariable("NEST_TAG");

            _channel.QueueBind(queue: _queueName, 
                exchange: ExchangeName, routingKey: routingKey + ".*");
            
            if (wantCushionMsgs)
            {
                string cushionIndex = Environment.GetEnvironmentVariable("NEST_CUSHION_INDEX");

                _channel.QueueBind(queue: _queueName, 
                    exchange: ExchangeName, routingKey: routingKey + "." + cushionIndex);                                
            }
        }
        
        public BasicGetResult GetMessage(
            bool noAck = false)
        {
            BasicGetResult result = _channel.BasicGet(_queueName, noAck);

            if (result != null && !noAck)
            {
                _channel.BasicAck(result.DeliveryTag, false);
            }

            return result;
        }
        
        public EventingBasicConsumer GetEventingConsumer(   
            bool autoAck = false)
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(queue: _queueName, 
                autoAck: autoAck, consumer: consumer);      
            return consumer;
        }
    }
}

