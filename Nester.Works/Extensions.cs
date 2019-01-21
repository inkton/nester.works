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
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Inkton.Nest.Cloud;
using Inkton.Nester.Logging;

namespace Inkton.Nester
{
    public static class Extensions 
    {
        public static IServiceCollection AddNester(
            this IServiceCollection services, 
            QueueMode mode = QueueMode.None, int serviceTimeoutSec = 180)
        {
            services.AddTransient<Runtime>(
                runtime => new Runtime(mode,                     
                    serviceTimeoutSec,
                    Enviorenment.Production));
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

        // These controller extensions help with generating standard
        // responses that can be extracted by the Nester.Library functions
        // The embedded type is prefixed with the type-name for type 
        // identification and safety 
        // https://github.com/inkton/nester.works/wiki#nestyt-standard-responses

        public static JsonResult NestResult(
            this ControllerBase controller,
            int code, 
            string text = null, 
            string notes = null,
            int htmlStatus = 200) 
        {
            Result<EmptyPayload> result = new Result<EmptyPayload>();
            result.Code = code;
            result.Text = text;
            result.Notes = notes;

            JsonResult jResult = new JsonResult(result);
            jResult.StatusCode = htmlStatus;

            return jResult;    
        }

        public static JsonResult NestResultSingle<T>(
            this ControllerBase controller,
            T data,
            int code = 0, 
            string text = null, 
            string notes = null,
            int htmlStatus = 200) where T : CloudObject, new()
        {
            Result<T> result = new Result<T>();
            result.Code = code;
            result.Text = text;
            result.Notes = notes;
            result.Data = new DataContainer<T>();
            result.Data.Payload = data;
            
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new DataContainerResolver(new T().GetObjectName());

            JsonResult jResult = new JsonResult(result, serializerSettings);
            jResult.StatusCode = htmlStatus;

            return jResult;            
        }

        public static JsonResult NestResultMultiple<T>(
            this ControllerBase controller,
            List<T> data,
            int code = 0, 
            string text = null, 
            string notes = null,
            int htmlStatus = 200) where T : CloudObject, new()
        {
            Result<List<T>> result = new Result<List<T>>();
            result.Code = 0;
            result.Text = text;
            result.Notes = notes;
            result.Data = new DataContainer<List<T>>();
            result.Data.Payload = data;

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new DataContainerResolver(new T().GetCollectionName());

            JsonResult jResult = new JsonResult(result, serializerSettings);
            jResult.StatusCode = htmlStatus;

            return jResult;
        }
    }
}
