using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Inkton.Nester.Logging;

namespace Microsoft.Extensions.Logging
{
    public static class NesterLoggerExtensions 
    {
        /// <summary>
        /// Adds a TraceSource logger named 'TraceSource' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
		/// <param name="append">if true new log entries are appended to the existing file.</param>	 
        public static ILoggingBuilder AddNester(
            this ILoggingBuilder builder,
			LogLevel minLevel = LogLevel.Warning,
            bool append = true)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<ILoggerProvider>(new NesterLoggerProvider(minLevel, append));
            return builder;
        }
       
        /// <summary>
        /// Adds a file logger.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="fileName">log file name.</param>
        /// <param name="append">if true new log entries are appended to the existing file.</param>	 
        public static ILoggerFactory AddNester(
            this ILoggerFactory factory, 
            LogLevel minLevel = LogLevel.Warning,
            bool append = true) 
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            factory.AddProvider(new NesterLoggerProvider(minLevel, append));
    	    return factory;
        }
    }
}
