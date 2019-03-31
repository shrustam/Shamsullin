using System.ServiceModel;
using Shamsullin.Common;
using Shamsullin.Wcf;

namespace Shamsullin.WcfCache
{
    class Program
    {
        static void Main()
        {
            Installer.OnInstall += () =>
            {
                Helpers.ConfigureSsl();
                Log.Instance.Info("SSL has configured");
            };

            // Start WCF
            var wcf = new ServiceHost(typeof(WcfCacheService));
            //var wcf = new ServiceHost(typeof(WcfCacheService), new Uri("https://127.0.0.1/wcfcache"));
            //var httpsBinding = new WebHttpBinding(WebHttpSecurityMode.Transport);
            //wcf.AddServiceEndpoint(typeof(WcfCacheService), httpsBinding, "").EndpointBehaviors.Add(new WcfRestBehavior());
            wcf.Open();

            Log.Instance.Info("WCF service has started");
            new WinService().Start();
        }
    }
}