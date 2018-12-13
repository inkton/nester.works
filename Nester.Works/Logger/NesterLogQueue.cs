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
    public abstract class NesterLogQueue<T> : IDisposable
    {
        protected readonly BlockingCollection<T> _entryQueue = new BlockingCollection<T>(1024);
        private readonly Task _processQueueTask;

        public NesterLogQueue()
        {
            _processQueueTask = Task.Factory.StartNew(
                Process,
                this,
                TaskCreationOptions.LongRunning);
        }

        virtual public void Clear()
        {
            _entryQueue.CompleteAdding();

            try {
                // the same as in ConsoleLogger
                _processQueueTask.Wait(1500);
            } catch (TaskCanceledException) 
            { }
            catch (AggregateException ex) 
                when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }
        }

        virtual public bool Add(T message)
        {
            if (_entryQueue.IsAddingCompleted)
                return false;
            
            try {
                _entryQueue.Add(message);
            } catch (InvalidOperationException) { }

            return true;
        }

        abstract protected void Handle(T message);

        virtual protected void Process()
        {
            foreach (var message in _entryQueue.GetConsumingEnumerable())
            {
                Handle(message);
            }            
        }        

        static private void Process(object state)
        {
            var logger = (NesterLogQueue<T>)state;
            logger.Process();
        }

        public void Dispose()
        {
            Clear();
        }        
    }

    public class NesterFileLogQueue : NesterLogQueue<string>
    {
        private readonly string LogFileName;
        object fileLock = new object();
        private readonly bool Append = true;

        public NesterFileLogQueue(bool append)
        {
            Append = append;

            string logFolder = Environment.GetEnvironmentVariable("NEST_FOLDER_LOG");

            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            string nestTag = Environment.GetEnvironmentVariable("NEST_TAG");
            string cushionIndex = Environment.GetEnvironmentVariable("NEST_CUSHION_INDEX");

            LogFileName = Path.Combine(logFolder, "nest." + nestTag + "." + cushionIndex + ".u" );
        }

        override protected void Handle(string message)
        {
            using (StreamWriter logWriter = new StreamWriter(LogFileName, Append))
            {
                logWriter.WriteLine(message);
            }
        }

        override public bool Add(string message)
        {
            if (!base.Add(message))
            {
                // use lock and write directly to a Nester?..
                lock (fileLock) {
                    System.IO.File.AppendAllText(LogFileName, message+"\n");
                }                
            }

            return true;
        }
    }    
}