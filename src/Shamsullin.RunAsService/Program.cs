using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Shamsullin.RunAsService
{
    class Program
    {
        private static Process _process;

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool SetConsoleCtrlHandler(CloseConsoleDelegate callback, bool add);

        protected delegate bool CloseConsoleDelegate(int eventType);

        protected static CloseConsoleDelegate Closer;

        private static bool ConsoleEventCallback(int eventType)
        {
            KillProcess();
            return false;
        }

        private static void KillProcess()
        {
            try
            {
                if (_process?.HasExited == false) _process.Kill();
            }
            catch
            {
            }
        }

        private static void StartProcess(string fileName, string arguments)
        {
            var fileInfo = new FileInfo(fileName);
            var psi = new ProcessStartInfo(fileName, arguments)
            {
                WorkingDirectory = fileInfo.DirectoryName,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                LoadUserProfile = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (_process = Process.Start(psi))
            {
                _process.WaitForExit();
                Environment.Exit(0);
            }
        }

        static void Main(string[] args)
        {
            if (!Environment.UserInteractive || args.Length > 0)
            {
                // Win Service
                Closer = ConsoleEventCallback;
                SetConsoleCtrlHandler(Closer, true);

                var fileName = args[0];
                if (!Path.IsPathRooted(fileName))
                {
                    var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    fileName = Path.Combine(dir, fileName);
                    if (!File.Exists(fileName)) throw new Exception($"File not found: {args[0]}");
                }

                var arguments = args.Length > 1 ? string.Join(" ", args.Skip(1)) : null;
                Console.WriteLine($"Running {fileName} {arguments}...");
                if (!Environment.UserInteractive)
                {
                    new WinService(() => StartProcess(fileName, arguments), KillProcess).Start();
                }
                else
                {
                    StartProcess(fileName, arguments);
                }
            }
            else
            {
                // Console
                Console.WriteLine("Usage:");
                Console.WriteLine("sc create <Name> binpath= \"<RunAsService.exe full path> <Application.exe> <Application Args>\" [start= auto]");
            }
        }
    }
}