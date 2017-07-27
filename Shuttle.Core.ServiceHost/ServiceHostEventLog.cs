using System.ComponentModel;
using System.Diagnostics;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Core.ServiceHost
{
    public class ServiceHostEventLog
    {
        private readonly EventLog _eventLog;

        public ServiceHostEventLog(string source)
        {
            Guard.AgainstNullOrEmptyString(source, "source");

            _eventLog = GetEventLog(source);
        }

        public static EventLog GetEventLog(string source)
        {
            var result = new EventLog();

            try
            {
                ((ISupportInitialize) result).BeginInit();
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, "Application");
                }
                ((ISupportInitialize) result).EndInit();

                result.Source = source;
                result.Log = "Application";
            }
            catch
            {
                result = null;
            }

            return result;
        }

        public void WrinteEntry(string message)
        {
            WrinteEntry(message, EventLogEntryType.Information);
        }

        public void WrinteEntry(string message, EventLogEntryType type)
        {
            _eventLog?.WriteEntry(message, type);
        }
    }
}