using System.ComponentModel.Composition;
using GitWorkflows.Package.Interfaces;
using GitWorkflows.Package.VisualStudio;
using GitWorkflows.Services;
using GitWorkflows.Services.Events;
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
        private GitWorkingTreeChangedEvent _workingTreeChangedEvent;

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

            _workingTreeChangedEvent.Subscribe(_ => PostExec());
        }
    }
}