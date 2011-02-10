using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GitWorkflows.Controls;

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
            vm.SelectionChanged();
        }

        private void ChangeList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hitItem = ChangeList.InputHitTest(e.GetPosition(ChangeList)) as DependencyObject;
            if (hitItem == null)
                return;

            var item = hitItem as ListViewItem ?? hitItem.GetVisualAncestor<ListViewItem>();
            if (item == null)
                return;

            // Select this item in the list view
            item.IsSelected = true;
        }
    }
}