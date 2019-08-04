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
using System.Text;
using System.Dynamic;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Inkton.Nest.Cloud;
using Inkton.Nester.Queue;
using Inkton.Nester.Notification;
using Inkton.Nest.Model;

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

    public enum Enviorenment
    {
        Production,
        Development,
        Integration
    }

    [Flags]
    public enum QueueMode : byte 
    {
        None = 0x0,            
        Client = 0x1,
        Server = 0x2,            
    }

    public class NesterServices
    {
        public delegate object ReceiveParser(IDictionary<string, object> headers, string message);

        private Enviorenment _enviorenment;
        private Chirpy _chirpy;
        private NesterQueueClient _queueClient;
        private NesterQueueServer _queueServer;
        private IDictionary<String, Object> _settings;
        private int _serviceTimeoutSec = 180;

        public NesterServices()
        {
            _enviorenment = Enviorenment.Production;
            Setup(QueueMode.None);
        }

        public NesterServices(QueueMode mode,
            int serviceTimeoutSec = 180,
            Enviorenment enviorenment = Enviorenment.Production)
        {
            _enviorenment = enviorenment;
            _serviceTimeoutSec = serviceTimeoutSec;

            Setup(mode);
        }

        public virtual void Setup(QueueMode mode)
        {
            string appFolder = Environment.GetEnvironmentVariable("NEST_FOLDER_APP");
            string appFileName = Path.Combine(appFolder, "app.json");
            FileStream fs = new FileStream(appFileName, FileMode.Open, FileAccess.Read);

            using (StreamReader sr = new StreamReader(fs))
            {
                string json = sr.ReadToEnd();
                _settings = JsonConvert.DeserializeObject<ExpandoObject>(json) 
                    as IDictionary<String, Object>;
            }
            
            if (_settings["collaboration"] != null)
            {
                var collab = _settings["collaboration"] as IDictionary<String, Object>;
                string token = collab["access_token"] as string;
                var inWebHook = collab["incoming_webhook"] as IDictionary<String, Object>;
                string channel = inWebHook["channel"] as string;

                _chirpy = new Chirpy(token, channel);
            }

            if ((mode & QueueMode.Client) == QueueMode.Client)
            {
                _queueClient = new NesterQueueClient(
                    RabbitMQ, _enviorenment);
            }
            if ((mode & QueueMode.Server) == QueueMode.Server)
            {
                _queueServer = new NesterQueueServer(
                    RabbitMQ, _enviorenment); 
            }
        }

        public int ServiceTimeoutSec 
        {
            get
            {
                return _serviceTimeoutSec;
            }
            set
            {
                _serviceTimeoutSec = value;
            }        
        }

        public string AppTag
        {
            get 
            {
                return _settings["tag"] as string;
            }           
        }

        public string ServicesPassword
        {
            get 
            {
                return _settings["services_password"] as string;
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

        public string ComponentId
        {
            get
            { 
                return NestTag + "." + CushionIndex.ToString(); 
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

        public Chirpy Chirpy
        {
            get 
            {   
                return _chirpy; 
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

        public IDictionary<String, Object> Settings
        {
            get
            {
                return _settings;
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
        
        public virtual Inkton.Nest.Model.Nest GetNest(string tag)
        {
            foreach (var nest in _settings["nests"] as List<dynamic>)
            {     
                if ((nest as IDictionary<String, Object>)["tag"] as string == tag)
                {
                    var serialized = JsonConvert.SerializeObject(nest);
                    return JsonConvert.DeserializeObject<Inkton.Nest.Model.Nest>(serialized);
                }
            }

            return null;
        }

        public virtual void Send<T>(T message, Inkton.Nest.Model.Nest nest,
            Type type = null, string correlationId = null, int cushion = -1)
        {    
            // The server sends a message to the client. The type 
            // header identify the type of message arrived.    

            _queueClient.Send(message, nest, type, correlationId, cushion);
        }

        public virtual T Receive<T>(bool checkType = false)
        {
            // The client receives the message, confirms
            // the type if necessary and serializes the
            // message.

            var message = _queueServer.GetMessage();

            T result = default(T);

            if (message != null) {
                IBasicProperties props = message.BasicProperties;
                _queueServer.LastCorrelationId = props.CorrelationId;

                byte[] body = message.Body;
                var process = true;

                if (checkType && 
                    props.Headers != null && 
                    props.Headers.ContainsKey("Type"))
                {
                    string messageType = Encoding.UTF8.GetString(
                        props.Headers["Type"] as byte[]);
                    Type returnType = typeof(T);

                    process = (returnType.GetType().FullName == messageType);
                }
                
                if (process)
                {
                    var messageBody = Encoding.UTF8.GetString(body);
                    result = JsonConvert.DeserializeObject<T>(messageBody);
                }
            }

            return result;
        }

        public virtual ResultSingle<T> ReceiveSingle<T>() where T : CloudObject, new() 
        {
            // The client sends back a result. the result has
            // a code and other detail to indicate whether the
            // result was a success. the data secion stores the
            // query result. The result is single if only one 
            // result is sent and multiple if it sends back a 
            // collection of data.

            var message = _queueServer.GetMessage();

            ResultSingle<T> result = null;

            if (message != null) {
                IBasicProperties props = message.BasicProperties;
                _queueServer.LastCorrelationId = props.CorrelationId;

                byte[] body = message.Body;

                T seed = new T();
                var messageBody = Encoding.UTF8.GetString(body);              
                result = ResultSingle<T>.ConvertObject(messageBody, seed);
            }
           
            return result;
        }

        public virtual ResultMultiple<T> ReceiveMultiple<T>() where T : CloudObject, new() 
        {
            // The client sends back a result. the result has
            // a code and other detail to indicate whether the
            // result was a success. the data secion stores the
            // query result. The result is single if only one 
            // result is sent and multiple if it sends back a 
            // collection of data.

            var message = _queueServer.GetMessage();

            ResultMultiple<T> result = null;

            if (message != null) {
                IBasicProperties props = message.BasicProperties;
                _queueServer.LastCorrelationId = props.CorrelationId;

                byte[] body = message.Body;

                T seed = new T();
                var messageBody = Encoding.UTF8.GetString(body);
                result = ResultMultiple<T>.ConvertObject(messageBody, seed);
            }
           
            return result;
        }
    }
}
