using log4net;
using log4net.Config;

namespace Shamsullin.Common.Logs
{
    public static class Log
    {
        static Log()
        {
            XmlConfigurator.Configure();
            Instance = LogManager.GetLogger(string.Empty);
        }

        public static ILog Instance { get; set; }
    }
}