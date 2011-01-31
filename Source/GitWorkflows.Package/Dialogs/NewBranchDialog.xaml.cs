using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace GitWorkflows.Package.Dialogs
{
    /// <summary>
    /// Interaction logic for NewBranchDialog.xaml
    /// </summary>
    [Export("NewBranchViewModel", typeof(DialogWindow))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NewBranchDialog
    {
        public NewBranchDialog()
        { InitializeComponent(); }
    }
}
