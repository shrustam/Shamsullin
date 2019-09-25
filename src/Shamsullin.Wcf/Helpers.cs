using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Shamsullin.Wcf
{
    public class Helpers
    {
        public static X509Certificate2 GetCertificate()
        {
            // Install Certificate
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Cast<X509Certificate2>();
            var result = certs.FirstOrDefault(x => x.HasPrivateKey);
            return result;
        }

        public static void ConfigureSsl(bool setCert = true, bool openPort = true)
        {
            if (setCert)
            {
                // Configure certificate for the application
                var cert = GetCertificate();
                if (cert == null)
                {
                    // Powershell New-SelfSignedCertificate -DnsName localhost -CertStoreLocation "cert:\LocalMachine\My"
                    var powershell = new Process();
                    powershell.StartInfo.FileName = @"C:\WINDOWS\System32\WindowsPowerShell\v1.0\powershell.exe";
                    powershell.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    powershell.StartInfo.Arguments = "New-SelfSignedCertificate -DnsName localhost -CertStoreLocation \"cert:\\LocalMachine\\My\"";
                    powershell.Start();
                    powershell.WaitForExit();
                    cert = GetCertificate();
                    Trace.WriteLine("SSL cert created");
                }

                var appid = GetAppId(Assembly.GetCallingAssembly());
                var proc = new Process();
                proc.StartInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "netsh.exe");
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                // netsh http show sslcert
                // netsh http delete sslcert ipport=0.0.0.0:443
                proc.StartInfo.Arguments = $"http add sslcert ipport=0.0.0.0:443 certhash={cert.Thumbprint} appid={{{appid}}}";
                proc.Start();
                proc.WaitForExit();
                Trace.WriteLine("SSL cert set for application");
            }

            // Open port
            if (openPort)
            {
                var proc = new Process();
                proc.StartInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "netsh.exe");
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.Arguments = "advfirewall firewall add rule name=\"Port 443 (HTTPS)\" protocol=TCP dir=in localport=443 action=allow";
                proc.Start();
                proc.WaitForExit();
                Trace.WriteLine("443 port opened");
            }
        }

        public static string GetAppId(Assembly assembly)
        {
            var attribute = (GuidAttribute) assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            var result = attribute.Value;
            return result;
        }
    }
}