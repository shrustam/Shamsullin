using System;
using System.ComponentModel;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using Shamsullin.Common;

namespace Shamsullin.WinService
{
    public class Service : ServiceBase
    {
        private IContainer components;
        private readonly Action _onStart;
        private readonly Action _onStop;

        public Service(Action onStart, Action onStop)
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
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
            Log.Instance.InfoFormat("[{0}] started as service", ServiceName);
            Log.Instance.InfoFormat("Server name: {0}", Environment.MachineName);
            ThreadPool.QueueUserWorkItem(delegate
            {
                _onStart();
            });
        }

        protected override void OnStop()
        {
            _onStop();
            Log.Instance.InfoFormat("[{0}] service stopped", ServiceName);
        }

        private void DomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException -= DomainUnhandledException;
            Log.Instance.Error("Unhandled exception", (Exception)e.ExceptionObject);
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
}
