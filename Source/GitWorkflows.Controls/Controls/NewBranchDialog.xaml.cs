using System.ComponentModel.Composition;
using GitWorkflows.Services;

namespace GitWorkflows.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for NewBranchDialog.xaml
    /// </summary>
    [ExportView("NewBranchViewModel", WindowTitle = "Git Workflows: Create Branch")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NewBranchDialog
    {
        public NewBranchDialog()
        { InitializeComponent(); }
    }
}
