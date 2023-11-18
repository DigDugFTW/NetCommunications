using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetClientWindowsForms
{
    public static class Program
    {
        public static IContainer Container { get; private set; }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Container = Bootstrapper.BuildContainer();
          

            using(var scope = Container.BeginLifetimeScope())
            {
                var shellView = scope.Resolve<ShellView>();
                Application.Run(shellView);
            }
            
        }
    }
}
