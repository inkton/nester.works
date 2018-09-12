using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Inkton.Nest.Cloud;
using Inkton.Nester.Logging;

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

        public static OkObjectResult NestResult(
            this ControllerBase controller,
            int code, 
            string text = null, 
            string notes = null) 
        {
            return controller.Ok(ResultFactory.Create(
                code, text, notes));
        }

        public static OkObjectResult NestResultSingle<T>(
            this ControllerBase controller,
            T data,
            int code = 0, 
            string text = null, 
            string notes = null ) where T : CloudObject, new()
        {
            return controller.Ok(ResultFactory.CreateSingle<T>(
                data, code, text, notes));
        }

        public static OkObjectResult NestResultMultiple<T>(
            this ControllerBase controller,
            List<T> data,
            int code = 0, 
            string text = null, 
            string notes = null ) where T : CloudObject, new()
        {
            return controller.Ok(ResultFactory.CreateMultiple<T>(
                data, code, text, notes));
        }
    }
}
