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
            services.AddTransient<NesterServices>(
                NesterServices => new NesterServices(mode,                     
                    serviceTimeoutSec,
                    Enviorenment.Production));
            return services;
        }

        public static IServiceCollection AddNesterMySQL<TContext> (
            this IServiceCollection services, 
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped, 
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped) where TContext : DbContext
        {
            NesterServices runtime = new NesterServices();
            services.AddDbContext<TContext>(options =>
               options.UseMySql(
                    string.Format(@"Server={0};database={1};uid={2};pwd={3};",
                         runtime.MySQL.Host,
                         runtime.MySQL.Resource,
                         runtime.MySQL.User,
                         runtime.MySQL.Password)),
                         contextLifetime, optionsLifetime);
            return services;
        }

        public static IServiceCollection AddNesterGeoraphyL<TContext>(
            this IServiceCollection services,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped) where TContext : DbContext
        {
            NesterServices runtime = new NesterServices();
            services.AddDbContext<TContext>(options =>
               options.UseMySql(
                    string.Format(@"Server={0};database={1};uid={2};pwd={3};",
                         runtime.MySQL.Host,
                         runtime.MySQL.Resource,
                         runtime.MySQL.User,
                         runtime.MySQL.Password)),
                         contextLifetime, optionsLifetime);
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

       public static JsonResult NestResult<T>(
            this ControllerBase controller,
            T code = default(T), 
            string notes = null,
            int htmlStatus = 200) 
                where T : System.Enum
        {
            Result<EmptyPayload> result = new Result<EmptyPayload>();
            result.Code = Convert.ToInt32(code);
            result.Text = Enum.GetName(typeof(T), code);
            result.Notes = notes;

            JsonResult jResult = new JsonResult(result);
            jResult.StatusCode = htmlStatus;

            return jResult;    
        }

        public static JsonResult NestResultSingle<D>(
            this ControllerBase controller,
            int code, string text, D data,
            string notes = null,
            int htmlStatus = 200) 
                where D : ICloudObject, new()
        {
            Result<D> result = new Result<D>();
            result.Code = code;
            result.Text = text;
            result.Notes = notes;
            result.Data = new DataContainer<D>();
            result.Data.Payload = data;
            
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new DataContainerResolver(new D().GetObjectName());

            JsonResult jResult = new JsonResult(result, serializerSettings);
            jResult.StatusCode = htmlStatus;

            return jResult;            
        }

        public static JsonResult NestResultSingle<T,D>(
            this ControllerBase controller,
            T code, D data,
            string notes = null,
            int htmlStatus = 200) 
                where T : System.Enum
                where D : ICloudObject, new()
        {
            return NestResultSingle(controller,
                Convert.ToInt32(code), 
                Enum.GetName(typeof(T), code),
                data, notes);         
        }

        public static JsonResult NestResultMultiple<D>(
            this ControllerBase controller,
            int code, string text, List<D> data,
            string notes = null,
            int htmlStatus = 200) 
                where D : ICloudObject, new()
        {
            Result<List<D>> result = new Result<List<D>>();
            result.Code = code;
            result.Text = text;
            result.Notes = notes;
            result.Data = new DataContainer<List<D>>();
            result.Data.Payload = data;

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new DataContainerResolver(new D().GetCollectionName());

            JsonResult jResult = new JsonResult(result, serializerSettings);
            jResult.StatusCode = htmlStatus;

            return jResult;
        }

        public static JsonResult NestResultMultiple<T,D>(
            this ControllerBase controller,
            T code, List<D> data,
            string notes = null,
            int htmlStatus = 200) 
                where T : System.Enum
                where D : ICloudObject, new()
        {
            return NestResultMultiple(controller,
                Convert.ToInt32(code), 
                Enum.GetName(typeof(T), code),
                data, notes, htmlStatus);
        }        
    }
}
