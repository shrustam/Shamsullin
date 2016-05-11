using System.Configuration;
using log4net;
using log4net.Config;
using Shamsullin.Common.Extensions;

namespace Shamsullin.Common
{
    public static class Log
    {
        static Log()
        {
			XmlConfigurator.Configure();
            Instance = LogManager.GetLogger(string.Empty);
            ConfigurationManager.AppSettings.AllKeys.ForEach(x => Instance.InfoFormat("{0}: {1}", x, ConfigurationManager.AppSettings[x]));
        }

        public static ILog Instance { get; private set; }
    }
}
