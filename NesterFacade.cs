using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using Inkton.Nest.Admin;
using Inkton.NesterWorks.Logging;

namespace Inkton.NesterWorks
{
    public struct NesterService
    {
        public string Host;
        public string User;
        public string Password;
        public string Resource;        
    }

    public class NesterFacade
    {
        private App _app;

        public NesterFacade()
        {
            string appFileName = Path.Combine(AppFolder, "app.json");
            FileStream fs = new FileStream(appFileName, FileMode.Open, FileAccess.Read);

            using (StreamReader sr = new StreamReader(fs)) 
            {
                string json = sr.ReadToEnd();
                _app = JsonConvert.DeserializeObject<App>(json);
            }
        }

        public App App
        {
            get { return _app; }
        }
        
        public int AppId
        {
            get {
                return int.Parse(Environment.GetEnvironmentVariable("NEST_APP_ID"));
                }            
        }

        public string AppTag
        {
            get {
                return Environment.GetEnvironmentVariable("NEST_APP_TAG");
                }            
        }

        public string AppName
        {
            get {
                return Environment.GetEnvironmentVariable("NEST_APP_NAME");
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

        public NesterService MySQL
        {
            get { 
                NesterService service = new NesterService();
                service.Host = Environment.GetEnvironmentVariable("NEST_MYSQL_HOST");
                service.User = _app.Tag;
                service.Password = _app.ServicesPassword;
                service.Resource = _app.Tag;
                return service;
                }
        }

        public NesterService RabbitMQ
        {
            get {
                NesterService service = new NesterService();
                service.Host = Environment.GetEnvironmentVariable("NEST_RABBITMQ_HOST");
                service.User = _app.Tag;
                service.Password = _app.ServicesPassword;
                service.Resource = "/";
                return service;
                }
        }
    }
}
