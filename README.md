# Introduction 
NesterWorks is a library used by nest.yt apps to access nest.yt PaaS services. The source code complies with .NET Standard 2.0.

## Dependencies

| Current Release   |  Dependency                 |
| ----------------- | --------------------------- |
| 1.5.3             | nester.model v1.0.1         |

# Getting Started

## 1.   Source code Install
```
git clone --branch v1.0.1 https://github.com/inkton/nester.model.git
git clone --branch v1.5.3 https://github.com/inkton/nester.works.git
cd nester.works
dotnet restore
dotnet build
```

## 2.   Installation via NuGet
```
dotnet add package Inkton.NesterWorks
or Install-Package Inkton.NesterWorks
```

## 3.   Runtime information
  
  The login details to access the services can be obtained by the runtime.
  
eg. MySQL Connection Details
```
    Runtime runtime = new Runtime();
    services.AddDbContext<CafeContext>(options =>
       options.UseMySql(
            string.Format(@"Server={0};database={1};uid={2};pwd={3};",
                 runtime.MySQL.Host,
                 runtime.MySQL.Resource,
                 runtime.MySQL.User,
                 runtime.MySQL.Password)
    ));`   
```
## 4.   Queue Service
  
  The platform provides a queue service to send messages between nest-cushions or the containers that handle execution.

eg. Queue Server
  
  The routing is performed by the QueueSendType runtime member. The queue receiver can identify the type of object received from the QueueSendType property.

```   
    Runtime runtime = new Runtime(QueueMode.Server);
    runtime.QueueSendType = "Order";    
    
    runtime.SendToNest(JsonConvert.SerializeObject(order), "stockallocator");
``` 
eg. Queue Client
```
        static object Parse(IDictionary<string, object> headers, string message)
        {
            if (headers != null && headers.ContainsKey("Type"))
            {
                switch(Encoding.Default.GetString(headers["Type"] as byte[]))
                {
                    case "Order":
                        return JsonConvert.DeserializeObject<Order>(message);
                }
            }

            return null;
        }
```
```
    Runtime runtime = new Runtime(QueueMode.Client);
    Runtime.ReceiveParser parsr = new Runtime.ReceiveParser(Parse);`   
    Order order = runtime.Receive(parsr) as Order;
```
## 5.  nest.yt Standard responses
   
  The nester.library client expects server responses in a standard JSON format. The server can generate responses using the following mechanism.

```
    using Cloud = Inkton.Nester.Cloud;

    Cloud.Result<Message> result = new Cloud.Result<Message>();

    result.SetSuccess("message", message);
```
