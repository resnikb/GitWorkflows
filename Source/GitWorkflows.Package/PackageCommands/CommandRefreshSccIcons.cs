using System.ComponentModel.Composition;
using GitWorkflows.Git;
using GitWorkflows.Package.Interfaces;
using GitWorkflows.Package.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandRefreshSccIcons : MenuCommand
    {
        [Import]
        private SourceControlProvider _sourceControlProvider;

        [Import]
        private ISolutionService _solutionService;

        [Import]
        private IRepositoryService _repositoryService;

        public CommandRefreshSccIcons() 
            : base(Constants.guidPackageCmdSet, Constants.cmdidRefreshSccIcons)
        {}

        protected override void Execute(object sender, OleMenuCmdEventArgs e)
        { _solutionService.RefreshSourceControlIcons(); }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            _sourceControlProvider.Activated += (sender, e) => PostExec();
            _sourceControlProvider.Deactivated += (sender, e) => PostExec();

            _repositoryService.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Status")
                    PostExec();
            };
        }
    }
}