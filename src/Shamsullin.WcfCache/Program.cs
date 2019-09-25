using System.Diagnostics;
using System.ServiceModel;
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
                Trace.WriteLine("SSL has configured");
            };

            // Start WCF
            var wcf = new ServiceHost(typeof(WcfCacheService));
            //var wcf = new ServiceHost(typeof(WcfCacheService), new Uri("https://127.0.0.1/wcfcache"));
            //var httpsBinding = new WebHttpBinding(WebHttpSecurityMode.Transport);
            //wcf.AddServiceEndpoint(typeof(WcfCacheService), httpsBinding, "").EndpointBehaviors.Add(new WcfRestBehavior());
            wcf.Open();

            Trace.WriteLine("WCF service has started");
            new WinService().Start();
        }
    }
}