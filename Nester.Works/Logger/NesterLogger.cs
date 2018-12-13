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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Inkton.Nester.Logging 
{
    public class NesterLogger : ILogger 
    {
        private readonly string logName;
        private readonly string nestTag;
        private readonly int cushionIndex;
        
        private readonly NesterLoggerProvider provider;

        public NesterLogger(string logName, NesterLoggerProvider provider) {
            this.logName = logName;
            this.nestTag = Environment.GetEnvironmentVariable("NEST_TAG");
            this.cushionIndex = Int32.Parse(Environment.GetEnvironmentVariable("NEST_CUSHION_INDEX"));
            this.provider = provider;
        }
        
        public IDisposable BeginScope<TState>(TState state) {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) {
            return logLevel>=provider.MinLevel;
        }

        string GetShortLogLevel(LogLevel logLevel) {
            switch (logLevel) {
                case LogLevel.Trace:
                    return "TRCE";
                case LogLevel.Debug:
                    return "DBUG";
                case LogLevel.Information:
                    return "INFO";
                case LogLevel.Warning:
                    return "WARN";
                case LogLevel.Error:
                    return "FAIL";
                case LogLevel.Critical:
                    return "CRIT";
            }
            return logLevel.ToString().ToUpper();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter) {
            if (!IsEnabled(logLevel)) {
                return;
            }

            if (formatter == null) {
                throw new ArgumentNullException(nameof(formatter));
            }
            
            string message = null;
            if (null != formatter) {
                message = formatter(state, exception);
            }

            DateTime now = DateTime.UtcNow;
            const long TicksPerMicrosecond = 10;
            long unixEpochTicks = (long)(now.Ticks - new DateTime(1970, 1, 1).Ticks);
            long unixEpochMicroSeconds = unixEpochTicks / TicksPerMicrosecond;
            
            // default formatting logic
            if (!string.IsNullOrEmpty(message)) 
            {
                provider.Log(unixEpochMicroSeconds, now.ToString("MM/dd/yyyy HH:mm:ss.ffffff"),
                        nestTag, cushionIndex, GetShortLogLevel(logLevel),
                        logName, eventId, message);
            }
        }
    }
}
