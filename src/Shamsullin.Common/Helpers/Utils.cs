using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using Shamsullin.Common.Extensions;

namespace Shamsullin.Common.Helpers
{
    public static class Utils
    {
        public static T GetArg<T>(string[] args, string name, T def)
        {
            var ix = args.LastIndexOf(name);
            if (ix < 0) return def;
            var result = args[ix+1].To<T>();
            return result;
        }

        public static void SetAffinity(int affinity)
        {
            if (affinity <= Environment.ProcessorCount)
            {
                Process.GetCurrentProcess().ProcessorAffinity = (IntPtr) (1 << affinity-1);
            }
        }

        public static void WaitInfinite()
        {
            while (true) Console.ReadKey();
        }

        public static Process GetProcessByCommanLine(string commandLine)
        {
            var path = commandLine.StartsWith("\"") ? commandLine.Substring(1, commandLine.IndexOf("\"", 1, StringComparison.Ordinal)) : commandLine.Split(' ')[0];
            if (string.IsNullOrWhiteSpace(path)) throw new Exception("Incorrect command line");
            var processName = Path.GetFileNameWithoutExtension(path);
            var result = Process.GetProcessesByName(processName).SingleOrDefault(x => GetCommandLine(x) == commandLine);
            return result;
        }

        public static string GetCommandLine(Process process)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = "+process.Id))
                using (var objects = searcher.Get())
                {
                    return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return null;
            }
        }

        public static Process StartHiddenProcess(string fileName, string arguments = null)
        {
            var psi = new ProcessStartInfo(fileName, arguments)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                LoadUserProfile = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            return Process.Start(psi);
        }
    }
}