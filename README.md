# Introduction 
NesterWorks is a library used by nest.yt apps to access nest.yt PaaS services.  

# Getting Started
## 1.	Installation process
```
dotnet add package Inkton.NesterWorks
or Install-Package Inkton.NesterWorks
```

## 2.	Software dependencies
The library depends on https://github.com/inkton/nest.model module.

## 3.  Runtime information
  
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

eg. Queue Server
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
