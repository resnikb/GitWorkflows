using System.Windows;

namespace GitWorkflows.Package
{
    /// <summary>
    /// Interaction logic for PendingChangesControl.xaml
    /// </summary>
    public partial class PendingChangesControl
    {
        public PendingChangesControl()
        { InitializeComponent(); }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format(System.Globalization.CultureInfo.CurrentUICulture, "We are inside {0}.button1_Click()", this.ToString()),
                            "Pending Changes");

        }
    }
}