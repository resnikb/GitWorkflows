using System.Windows;
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

        private void ChangeList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as PendingChangeViewModel;
            if (item != null)
            {
                var vm = (PendingChangesWindow)DataContext;
                if (vm.CommandViewDifferences.CanExecute(null))
                    vm.CommandViewDifferences.Execute(null);
            }
        }
    }
}