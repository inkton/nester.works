using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Inkton.Nester.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Inkton.Nester
{
    public static class Extensions 
    {
        public static IServiceCollection AddNester(
            this IServiceCollection services, 
            QueueMode mode = QueueMode.None, int serviceTimeoutSec = 50)
        {
            services.AddTransient<Runtime>(
                runtime => new Runtime(mode, serviceTimeoutSec));
            return services;
        }

        public static IServiceCollection AddNesterMySQL<TContext> (
            this IServiceCollection services, 
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped, 
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped) where TContext : DbContext
        {
            Runtime runtime = new Runtime();
            services.AddDbContext<TContext>(options =>
               options.UseMySql(
                    string.Format(@"Server={0};database={1};uid={2};pwd={3};",
                         runtime.MySQL.Host,
                         runtime.MySQL.Resource,
                         runtime.MySQL.User,
                         runtime.MySQL.Password, contextLifetime, optionsLifetime)
            ));
            return services;
        }

        public static ILoggingBuilder AddNesterLog(
            this ILoggingBuilder builder,
			LogLevel minLevel = LogLevel.Warning,
            bool append = true)
        {
            builder.Services.AddSingleton<ILoggerProvider>(
                new NesterLoggerProvider(minLevel, append));
            return builder;
        }
       
        public static ILoggerFactory AddNesterLog(
            this ILoggerFactory factory, 
            LogLevel minLevel = LogLevel.Warning,
            bool append = true) 
        {
            factory.AddProvider(
                new NesterLoggerProvider(minLevel, append));
    	    return factory;
        }
    }
}
