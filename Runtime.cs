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
using System.IO;
using System.Dynamic;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Inkton.Nester.Models;
using Inkton.Nester.Queue;

namespace Inkton.Nester
{
    public struct NesterService
    {
        public string Host;
        public string User;
        public string Password;
        public string Resource;      
        public int TimeoutSec;  
    }

    [Flags]
    public enum QueueMode : byte 
    {
        None = 0x0,            
        Client = 0x1,
        Server = 0x2,            
    }

    public class Runtime
    {
        public delegate object ReceiveParser(IDictionary<string, object> headers, string message);

        private NesterQueueClient _queueClient;
        private NesterQueueServer _queueServer;        
        private ExpandoObject _settings;
        private int _serviceTimeoutSec;

        public Runtime(QueueMode mode = QueueMode.None, int serviceTimeoutSec = 50)
        {
            string appFolder = Environment.GetEnvironmentVariable("NEST_FOLDER_APP");
            string appFileName = Path.Combine(appFolder, "app.json");
            FileStream fs = new FileStream(appFileName, FileMode.Open, FileAccess.Read);

            _serviceTimeoutSec = serviceTimeoutSec;

            using (StreamReader sr = new StreamReader(fs))
            {
                string json = sr.ReadToEnd();
                _settings = JsonConvert.DeserializeObject<ExpandoObject>(json);
            }
            
            if ((mode & QueueMode.Client) == QueueMode.Client)
            {
                _queueClient = new NesterQueueClient(RabbitMQ);
            }
            if ((mode & QueueMode.Server) == QueueMode.Server)
            {
                _queueServer = new NesterQueueServer(RabbitMQ);            
            }
        }

        public string AppTag
        {
            get 
            {
                return Settings["tag"] as string;
            }           
        }

        public string ServicesPassword
        {
            get 
            {
                return Settings["services_password"] as string;
            }            
        }

        public string AppFolder
        {
            get 
            {
                return Environment.GetEnvironmentVariable("NEST_FOLDER_APP");
            }            
        }

        public string NestTag
        {
            get
            { 
                return Environment.GetEnvironmentVariable("NEST_TAG"); 
            }
        }

        public int CushionIndex
        {
            get 
            {
                return int.Parse(Environment.GetEnvironmentVariable("NEST_CUSHION_INDEX"));
            }            
        }

        public int ContactId
        {
            get
            {
                return int.Parse(Environment.GetEnvironmentVariable("NEST_CONTACT_ID"));
            }            
        }

        public string ContactEmail
        {
            get
            { 
                return Environment.GetEnvironmentVariable("NEST_CONTACT_EMAIL"); 
            }
        }

        public string QueueSendType
        {
            set
            {
                Dictionary<string, object> headers = 
                    new Dictionary<string, object>();
                headers["Type"] = value;
                _queueServer.Headers = headers;
            }
        }

        public NesterQueueClient QueueClient
        {
            get 
            {   
                return _queueClient; 
            }
        }

        public NesterQueueServer QueueServer
        {
            get 
            { 
                return _queueServer; 
            }
        }

        public void SendToNest(string message, string tag, int cushion = -1)
        {
            Nest target = new Nest();

            foreach (var nest in Settings["nests"] as List<dynamic>)
            {
                if ((nest as IDictionary<String, Object>)["tag"] as string == tag)
                {
                    Inkton.Nester.Cloud.Object.CopyExpandoPropertiesTo(nest, target);
                    _queueServer.Send(message, target, cushion);
                    break;
                }
            }            
        }   

        public object Receive(ReceiveParser parse)
        {
            BasicGetResult result = _queueClient.GetMessage();
            object payload = null;

            if (result != null) 
            {
                IBasicProperties props = result.BasicProperties;
                byte[] body = result.Body;
                var message = System.Text.Encoding.Default.GetString(body);
                payload = parse(props.Headers, message);
            }

            return payload;
        }

        public IDictionary<String, Object> Settings
        {
            get
            {
                return _settings as IDictionary<String, Object>;
            }
        }

        public NesterService MySQL

        {
            get 
            { 
                NesterService service = new NesterService();
                service.Host = Environment.GetEnvironmentVariable("NEST_MYSQL_HOST");
                service.User = AppTag;
                service.Password = ServicesPassword;
                service.Resource = AppTag;
                service.TimeoutSec = _serviceTimeoutSec;
                return service;
            }
        }

        public NesterService RabbitMQ
        {
            get 
            {
                NesterService service = new NesterService();
                service.Host = Environment.GetEnvironmentVariable("NEST_RABBITMQ_HOST");
                service.User = AppTag;
                service.Password = ServicesPassword;
                service.Resource = "/";
                service.TimeoutSec = _serviceTimeoutSec;
                return service;
            }
        }
    }
}
