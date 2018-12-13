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

        public NesterQueueRPCServer CreateRPCEndpoint(object sink)
        {
            return new NesterQueueRPCServer(sink,
                new Subscription(DefaultChannel.Model, QueueName)
            );
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

