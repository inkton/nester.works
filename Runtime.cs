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
    }

    public class Runtime
    {
        private ExpandoObject _settings;

        public Runtime()
        {
            string appFolder = Environment.GetEnvironmentVariable("NEST_FOLDER_APP");
            string appFileName = Path.Combine(appFolder, "app.json");
            FileStream fs = new FileStream(appFileName, FileMode.Open, FileAccess.Read);

            using (StreamReader sr = new StreamReader(fs))
            {
                string json = sr.ReadToEnd();
                _settings = JsonConvert.DeserializeObject<ExpandoObject>(json);
            }
        }

        public string AppTag
        {
            get {
                return Environment.GetEnvironmentVariable("NEST_APP_TAG");
                }            
        }

        public string ServicesPassword
        {
            get {
                return Environment.GetEnvironmentVariable("NEST_SERVICES_PASSWORD");
                }            
        }

        public string AppFolder
        {
            get {
                return Environment.GetEnvironmentVariable("NEST_FOLDER_APP");
                }            
        }

        public string NestTag
        {
            get { 
                return Environment.GetEnvironmentVariable("NEST_TAG"); 
                }
        }

        public int CushionIndex
        {
            get {
                return int.Parse(Environment.GetEnvironmentVariable("NEST_CUSHION_INDEX"));
                }            
        }

        public int ContactId
        {
            get {
                return int.Parse(Environment.GetEnvironmentVariable("NEST_CONTACT_ID"));
                }            
        }

        public string ContactEmail
        {
            get { 
                return Environment.GetEnvironmentVariable("NEST_CONTACT_EMAIL"); 
                }
        }
                
        public void SendToNest(string message, string tag, int cushion = -1)
        {
            NesterQueueServer queueServer = new NesterQueueServer(RabbitMQ);            
            Nest target = new Nest();

            foreach (var nest in Settings["nests"] as List<dynamic>)
            {
                if ((nest as IDictionary<String, Object>)["tag"] as string == tag)
                {
                    Inkton.Nester.Cloud.Object.CopyExpandoPropertiesTo(nest, target);
                    queueServer.Send(message, target, cushion);
                    break;
                }
            }            
        }   

        public T Receive<T>(bool noAck = false)
        {
            NesterQueueClient queueClient = new 
                NesterQueueClient(RabbitMQ);
            BasicGetResult result = queueClient.GetMessage(noAck);
            T payload = default(T);

            if (result != null) 
            {
                IBasicProperties props = result.BasicProperties;
                byte[] body = result.Body;
                var messageJson = System.Text.Encoding.Default.GetString(body);
                payload = JsonConvert.DeserializeObject<T>(messageJson);
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
            get { 
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
            get {
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
