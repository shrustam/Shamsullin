using System.Collections;
using System.Threading;
using log4net;
using log4net.Config;

namespace Shamsullin.Common
{
    /// <summary>
    /// Loggers separated by thread name.
    /// </summary>
    public static class ThreadLog
    {
        private static ILog _logDefault;
        private static readonly Hashtable Logs = new Hashtable();

        static ThreadLog()
        {
            XmlConfigurator.Configure();
            _logDefault = LogManager.GetLogger(string.Empty);
        }

        /// <summary>
        /// Get current instance or set the default instance.
        /// </summary>
        public static ILog Instance
        {
            get
            {
                var threadName = Thread.CurrentThread.Name;
                if (string.IsNullOrEmpty(threadName)) return _logDefault;
                var result = Logs[threadName] as ILog;
                if (result != null) return result;
                lock(Logs) Logs[threadName] = result = LogManager.GetLogger(threadName);
                return result;
            }
            set { _logDefault = value; }
        }
    }
}