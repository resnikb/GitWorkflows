using System.ComponentModel.Composition;
using GitWorkflows.Controls.ViewModels;
using GitWorkflows.Services;

namespace GitWorkflows.Application
{
    [Export]
    class ShellViewModel : ViewModel
    {
        [Import]
        public PendingChangesViewModel PendingChanges
        { get; private set; }

        [ImportingConstructor]
        public ShellViewModel(IRepositoryService repositoryService)
        {
            repositoryService.OpenRepositoryAt(@"C:\GitProjects\test");
        }
    }
}