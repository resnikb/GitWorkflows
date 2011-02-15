using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GitWorkflows.Controls.ViewModels;
using GitWorkflows.Services;

namespace GitWorkflows.Controls.Controls
{
    /// <summary>
    /// Interaction logic for PendingChangesControl.xaml
    /// </summary>
    [ExportView(typeof(PendingChangesViewModel))]
    public partial class PendingChangesControl
    {
        private PendingChangesViewModel ViewModel
        {
            get { return (PendingChangesViewModel)DataContext; }
        }

        public PendingChangesControl()
        { InitializeComponent(); }

        private void ChangeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectionChanged(ChangeList.SelectedItems);
        }

        private void ChangeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as StatusViewModel;
            if (item != null)
            {
                ICommand commandViewDifferences = ((dynamic)ViewModel).CommandViewDifferences;
                if (commandViewDifferences.CanExecute(null))
                    commandViewDifferences.Execute(null);
            }
        }
    }
}