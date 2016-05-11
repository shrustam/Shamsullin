using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace Shamsullin.WinService
{
    [RunInstaller(true)]
    public class Installer : System.Configuration.Install.Installer
    {
        private readonly IContainer components = null;

        public static string ServiceName { get; set; }

        public static string DisplayName { get; set; }

        public static string Description { get; set; }

        static Installer()
        {
            var assembly = Assembly.GetExecutingAssembly();
            ServiceName = DisplayName = assembly.GetName().Name;
            Description = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).OfType<AssemblyDescriptionAttribute>().First().Description;
        }

        public Installer()
        {
            InitializeComponent();
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
            Installers.AddRange(new System.Configuration.Install.Installer[]
            {
                new ServiceProcessInstaller
                {
                    Password = null,
                    Username = null
                },
                new ServiceInstaller
                {
                    Description = Description,
                    DisplayName = DisplayName,
                    ServiceName = ServiceName
                }
            });
        }
    }
}
