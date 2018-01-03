﻿using System;
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

        public Runtime(QueueMode mode = QueueMode.None)
        {
            string appFolder = Environment.GetEnvironmentVariable("NEST_FOLDER_APP");
            string appFileName = Path.Combine(appFolder, "app.json");
            FileStream fs = new FileStream(appFileName, FileMode.Open, FileAccess.Read);

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
                return service;
            }
        }
    }
}