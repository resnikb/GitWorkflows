using System;
using System.ComponentModel.Composition;
using System.Linq;
using GitWorkflows.Controls.ViewModels;
using GitWorkflows.Package.VisualStudio;
using GitWorkflows.Services;
using GitWorkflows.Services.Events;
using Microsoft.VisualStudio.Shell;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandNewBranch : MenuCommand
    {
        [Import]
        private IBranchManager _branchManager;

        [Import]
        private IViewService _viewService;

        [Import]
        private GitBranchCollectionChangedEvent BranchCollectionChangedEvent
        {
            set { value.Subscribe(branchManager => _isEnabled = branchManager.Branches.Any()); }
        }

        private volatile bool _isEnabled;

        public CommandNewBranch()
            : base(Constants.guidPackageCmdSet, Constants.cmdidNewBranch)
        {}

        protected override void Execute(object sender, OleMenuCmdEventArgs e)
        {
            _viewService.ShowDialog(
                new NewBranchViewModel(_branchManager.CurrentBranch.Name)
                {
                    NewBranchName = e.InValue as string
                },
                onSuccess: vm => _branchManager.Create(vm.NewBranchName, vm.CheckoutAfterCreating)
            );
        }

        protected override void DoUpdateStatus(object sender, EventArgs e)
        { Command.Enabled = _isEnabled; }
    }
}