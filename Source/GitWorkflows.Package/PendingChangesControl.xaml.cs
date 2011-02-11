using System.Windows.Controls;

namespace GitWorkflows.Package
{
    /// <summary>
    /// Interaction logic for PendingChangesControl.xaml
    /// </summary>
    public partial class PendingChangesControl
    {
        public PendingChangesControl()
        { InitializeComponent(); }

        private void ChangeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = (PendingChangesWindow)DataContext;
            vm.SelectionChanged(ChangeList.SelectedItems);
        }
    }
}