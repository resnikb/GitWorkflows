using System;
using System.ComponentModel.Composition;
using GitWorkflows.Package.Git;
using GitWorkflows.Package.ViewModels;
using GitWorkflows.Package.VisualStudio;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandNewBranch : MenuCommand
    {
        [Import]
        private IGitService _gitService;

        [Import]
        private IBranchManager _branchManager;

        [Import]
        private IDialogService _dialogService;

        public CommandNewBranch()
            : base(Constants.guidPackageCmdSet, Constants.cmdidNewBranch)
        {}

        protected override void Execute(object sender, EventArgs e)
        {
            var data = new NewBranchViewModel(_branchManager.CurrentBranch.Name);
            if (_dialogService.ShowDialog(data) != true)
                return;

            _branchManager.Create(data.NewBranchName, data.CheckoutAfterCreating);
        }

        protected override void DoUpdateStatus(object sender, EventArgs e)
        { Command.Enabled = _gitService.IsRepositoryOpen; }
    }
}