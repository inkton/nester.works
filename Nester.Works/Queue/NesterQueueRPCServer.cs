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
using System.Reflection;
using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;

namespace Inkton.Nester.Queue
{
    public class NesterQueueRPCServer : SimpleRpcServer
    {
        private object _sink;

        public NesterQueueRPCServer(object sink, Subscription subscription)
            :base(subscription) { _sink = sink; }

        public override byte[] HandleSimpleCall(
            bool isRedelivered, IBasicProperties requestProperties, 
            byte[] body, out IBasicProperties replyProperties)
        {
            replyProperties = requestProperties;
            replyProperties.MessageId = Guid.NewGuid().ToString();

            if (requestProperties.Headers.ContainsKey("Function"))
            {
                string method = Encoding.UTF8.GetString(
                    requestProperties.Headers["Function"] as byte[]);

                if (method != null)
                {
                    MethodInfo mi = _sink.GetType().GetMethod(method);                
                    return mi.Invoke(_sink, new object[] { body }) as byte[];
                }
            }

            return null;
        }
    }
}

