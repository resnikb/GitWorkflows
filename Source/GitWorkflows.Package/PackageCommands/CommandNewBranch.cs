using System;
using System.ComponentModel.Composition;
using System.Linq;
using GitWorkflows.Package.Common;
using GitWorkflows.Package.Interfaces;
using GitWorkflows.Package.ViewModels;
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
            var data = new NewBranchViewModel(_branchManager.CurrentBranch.Name);

            var args = e.InValue as EventArgs<string>;
            if (args != null)
                data.NewBranchName = args.Value;

            if (_dialogService.ShowDialog(data) != true)
                return;

            _branchManager.Create(data.NewBranchName, data.CheckoutAfterCreating);
        }

        protected override void DoUpdateStatus(object sender, EventArgs e)
        { Command.Enabled = _branchManager.Branches.Any(); }
    }
}