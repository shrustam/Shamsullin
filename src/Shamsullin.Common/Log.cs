using log4net;
using log4net.Config;
using System.Configuration;

namespace Shamsullin.Common
{
    public static class Log
    {
        static Log()
        {
            XmlConfigurator.Configure();
            Instance = LogManager.GetLogger(string.Empty);
            //foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            //{
            //    Instance.InfoFormat("{0}: {1}", key, ConfigurationManager.AppSettings[key]);
            //}
        }

        public static ILog Instance { get; set; }
    }
}