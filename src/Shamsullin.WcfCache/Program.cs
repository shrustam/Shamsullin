using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Shamsullin.Common;

namespace Shamsullin.WcfCache
{
    class Program
    {
        static void Main()
        {
            // Install Certificate
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Cast<X509Certificate2>();
            var cert = certs.First(x => x.HasPrivateKey);

            var appid = GetAppId();
            var proc = new Process();
            proc.StartInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "netsh.exe");
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.Arguments = $"http add sslcert ipport=127.0.0.1:443 certhash={cert.Thumbprint} appid={{{appid}}}";
            proc.Start();
            proc.WaitForExit();
            Log.Instance.Info($"Certificate {cert.Thumbprint} installed");

            // Start WCF
            var wcf = new ServiceHost(typeof(WcfCacheService));
            //var wcf = new ServiceHost(typeof(WcfCacheService), new Uri("https://127.0.0.1/wcfcache"));
            //var httpsBinding = new WebHttpBinding(WebHttpSecurityMode.Transport);
            //wcf.AddServiceEndpoint(typeof(WcfCacheService), httpsBinding, "").EndpointBehaviors.Add(new WcfRestBehavior());
            wcf.Open();

            Log.Instance.Info("WCF service has started");
            new WinService().Start();
        }

        private static string GetAppId()
        {
            var assembly = typeof(Program).Assembly;
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            var result = attribute.Value;
            return result;
        }
    }
}