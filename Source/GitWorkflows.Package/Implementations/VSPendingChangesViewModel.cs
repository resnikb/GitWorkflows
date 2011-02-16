using System.ComponentModel.Composition;
using GitWorkflows.Controls.ViewModels;
using GitWorkflows.Package.Interfaces;
using GitWorkflows.Services;
using GitWorkflows.Services.Events;

namespace GitWorkflows.Package.Implementations
{
    [Export(typeof(PendingChangesViewModel))]
    class VSPendingChangesViewModel : PendingChangesViewModel
    {
        [Import]
        private ISolutionService _solutionService;

        [ImportingConstructor]
        public VSPendingChangesViewModel(IRepositoryService repositoryService, IFileIconService iconService, GitWorkingTreeChangedEvent workingTreeChangedEvent) 
            : base(repositoryService, iconService, workingTreeChangedEvent)
        {}

        protected override StatusViewModel CreateStatusViewModel(IRepositoryService repositoryService, IFileIconService iconService, Git.Status s)
        {
            var status = base.CreateStatusViewModel(repositoryService, iconService, s);
            status.ProjectName = _solutionService.GetNameOfParentProject(s.FilePath);
            return status;
        }
    }
}