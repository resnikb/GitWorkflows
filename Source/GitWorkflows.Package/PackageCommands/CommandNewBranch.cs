using System;
using System.ComponentModel.Composition;
using System.Linq;
using GitWorkflows.Controls.ViewModels;
using GitWorkflows.Package.Interfaces;
using GitWorkflows.Package.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandNewBranch : MenuCommand
    {
        [Import]
        private IBranchManager _branchManager;

        [Import]
        private IDialogService _dialogService;

        public CommandNewBranch()
            : base(Constants.guidPackageCmdSet, Constants.cmdidNewBranch)
        {}

        protected override void Execute(object sender, OleMenuCmdEventArgs e)
        {
            var data = new NewBranchViewModel(_branchManager.CurrentBranch.Name)
            {
                NewBranchName = e.InValue as string
            };

            if (_dialogService.ShowDialog(data) != true)
                return;

            _branchManager.Create(data.NewBranchName, data.CheckoutAfterCreating);
        }

        protected override void DoUpdateStatus(object sender, EventArgs e)
        { Command.Enabled = _branchManager.Branches.Any(); }
    }
}