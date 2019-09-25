using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Shamsullin.RunAsService
{
    public class WinService : ServiceBase
    {
        private IContainer components;
        private readonly Action _onStart;
        private readonly Action _onStop;

        public WinService(Action onStart = null, Action onStop = null)
        {
            _onStart = onStart;
            _onStop = onStop;
            InitializeComponent();
            ServiceName = Assembly.GetExecutingAssembly().GetName().Name;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
        }

        protected override void OnStart(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += DomainUnhandledException;
            Trace.WriteLine($"[{ServiceName}] started as service");
            Trace.WriteLine($"Server name: {Environment.MachineName}");
            if (_onStart != null) Task.Run(() => _onStart());
        }

        protected override void OnStop()
        {
            _onStop?.Invoke();
            Trace.WriteLine($"[{ServiceName}] service stopped");
        }

        private void DomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException -= DomainUnhandledException;
            Trace.WriteLine(e.ExceptionObject);
            OnStop();
        }

        public void Start()
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine("Press S to stop, R to rerun...");
                OnStart(null);
                while (true)
                {
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.S:
                            Console.WriteLine();
                            OnStop();
                            break;
                        case ConsoleKey.R:
                            Console.WriteLine();
                            OnStart(null);
                            break;
                    }
                }
            }
            else
            {
                Run(this);
            }
        }
    }

    [RunInstaller(true)]
    public class Installer : System.Configuration.Install.Installer
    {
        private readonly IContainer components = null;

        public static string ServiceName { get; set; }

        public static string DisplayName { get; set; }

        public static string Description { get; set; }

        public static Action OnInstall;

        static Installer()
        {
            var assembly = Assembly.GetExecutingAssembly();
            ServiceName = DisplayName = assembly.GetName().Name;
            Description = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).OfType<AssemblyDescriptionAttribute>().First().Description;
        }

        public Installer()
        {
            InitializeComponent();
            OnInstall?.Invoke();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Installers.AddRange(new System.Configuration.Install.Installer[]
            {
                new ServiceProcessInstaller {Account = ServiceAccount.LocalSystem},
                new ServiceInstaller {Description = Description, DisplayName = DisplayName, ServiceName = ServiceName}
            });
        }
    }
}