using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;

namespace Inkton.Nester.Queue
{    
    public class NesterQueueServer : NesterQueueExchange
    {        
        private readonly string QueueName = Environment
            .GetEnvironmentVariable("NEST_TAG");

        public NesterQueueServer(NesterService service,
            bool durable = false, bool autoDelete = true)
            :base(service, durable, autoDelete) 
        {
            DefaultChannel.QueueDeclare(QueueName);

            // The generic nest endpoint
            DefaultChannel.QueueBind(QueueName + ".*");

            // The specific nest cushion endpoint
            string cushionIndex = Environment.GetEnvironmentVariable("NEST_CUSHION_INDEX");
            DefaultChannel.QueueBind(QueueName + "." + cushionIndex);
        }

        public BasicGetResult GetMessage(
            bool noAck = true)
        {
            BasicGetResult result = DefaultChannel
                .Model.BasicGet(QueueName, noAck);

            if (result != null && !noAck)
            {
                DefaultChannel.Model
                    .BasicAck(result.DeliveryTag, false);
            }

            return result;
        }

        public Subscription CreateSubscription()
        {
            var subscription = new Subscription(
                DefaultChannel.Model, QueueName);

            return subscription;
        }

        public EventingBasicConsumer CreateConsumer(
            bool autoAck = true)
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(
                DefaultChannel.Model);

            DefaultChannel.Model.BasicConsume(queue: QueueName, 
                autoAck: autoAck, consumer: consumer);

            return consumer;
        }
    }
} 

