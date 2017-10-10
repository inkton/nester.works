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