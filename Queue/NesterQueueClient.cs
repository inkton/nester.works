using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Collections.Generic;

using Inkton.Nest.Admin;

namespace Inkton.NesterWorks.Queue
{
    public class NesterQueueClient : NesterQueue
    {
        public NesterQueueClient(bool wantCushionMsgs = false,
            bool durable = false, bool autoDelete = false)
            :base(durable, autoDelete)
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
            bool noAck = true)
        {
            return _channel.BasicGet(_queueName, noAck);
        }
        
        public EventingBasicConsumer GetEventingConsumer(   
            bool autoAck = true)
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(queue: _queueName, 
                autoAck: autoAck, consumer: consumer);            
            return consumer;
        }
    }
}

