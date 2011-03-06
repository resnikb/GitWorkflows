using GitWorkflows.Services;

namespace GitWorkflows.Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ExportView(typeof(ShellViewModel))]
    public partial class MainWindow
    {
        public MainWindow()
        { InitializeComponent(); }
    }
}
