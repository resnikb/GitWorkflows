using System;
using System.ComponentModel.Composition;
using GitWorkflows.Package.Dialogs;
using GitWorkflows.Package.VisualStudio;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandNewBranch : MenuCommand
    {
        [Import]
        private ISolutionService _solutionService;

        [Import]
        private IDialogService _dialogService;

        public CommandNewBranch()
            : base(Constants.guidPackageCmdSet, Constants.cmdidNewBranch)
        {}

        protected override void Execute(object sender, EventArgs e)
        {
            var data = new NewBranchViewModel("xyz");
            if (_dialogService.ShowDialog(data) != true)
                return;
        }

        protected override void DoUpdateStatus(object sender, EventArgs e)
        { Command.Enabled = _solutionService.IsControlledByGit; }
    }
}