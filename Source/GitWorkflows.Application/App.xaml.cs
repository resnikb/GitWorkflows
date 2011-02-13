using System;
using System.Windows;

namespace GitWorkflows.Application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                var app = new App();
                app.Run();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private App()
        {
            var bootstrapper = new GitWorkflowsBootstrapper();
            bootstrapper.Run();  
        }
    }
}
