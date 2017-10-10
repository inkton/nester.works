#region License
/*
 * Nester Logging provider based on 
 * NReco file logging provider (https://github.com/nreco/logging)
 * Copyright 2017 Vitaliy Fedorchenko
 * Distributed under the MIT license
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Inkton.NesterWorks.Logging 
{	
	/// <summary>
	/// Generic Nester logger provider.
	/// </summary>
	public class NesterLoggerProvider : ILoggerProvider, IDisposable
	{
		private readonly ConcurrentDictionary<string, NesterLogger> _loggers =
			new ConcurrentDictionary<string, NesterLogger>();
		private NesterFileLogQueue _fileQueue =
			new NesterFileLogQueue(true);
		public LogLevel MinLevel { get; set; } = LogLevel.Warning;

		public NesterLoggerProvider()
		{
		}

		public NesterLoggerProvider(LogLevel minLevel)
			:this()
		{
			MinLevel = minLevel;
		}

		public NesterLoggerProvider(LogLevel minLevel, bool append)
		{
			MinLevel = minLevel;			
			_fileQueue = new NesterFileLogQueue(append);
		}

		public ILogger CreateLogger(string categoryName) 
		{
			return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
		}

		private NesterLogger CreateLoggerImplementation(string name)
		{
			return new NesterLogger(name, this);
		}

		/// <summary>
		/// Turn a string into a CSV cell output
		/// </summary>
		/// <param name="str">String to output</param>
		/// <returns>The CSV cell formatted string</returns>
		private static string StringToCSVCell(string str)
		{
			bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
			if (mustQuote)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("\"");
				foreach (char nextChar in str)
				{
					sb.Append(nextChar);
					if (nextChar == '"')
						sb.Append("\"");
				}
				sb.Append("\"");
				return sb.ToString();
			}

			return str;
		}		

		public void Log(long unixEpoch, string utcTime,
			string nestTag, int cushionIndex, string level, 
			string name, EventId eventId, string message)
		{
			var logBuilder = new StringBuilder();
			logBuilder.Append(unixEpoch);					
			logBuilder.Append(",");
			logBuilder.Append(utcTime);
			logBuilder.Append(',');
			logBuilder.Append(nestTag);
			logBuilder.Append(',');
			logBuilder.Append(cushionIndex);
			logBuilder.Append(',');					
			logBuilder.Append(level);
			logBuilder.Append(",");
			logBuilder.Append(name);
			logBuilder.Append(",");
			logBuilder.Append(eventId);
			logBuilder.Append(",");
			logBuilder.Append(StringToCSVCell(message));

			_fileQueue.Add(logBuilder.ToString());
		}

		public void Dispose()
		{
			_loggers.Clear();
			if (_fileQueue != null)
			{
				_fileQueue.Dispose();
			}
		}				
	}
}
