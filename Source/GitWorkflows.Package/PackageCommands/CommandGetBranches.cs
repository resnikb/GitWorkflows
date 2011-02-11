using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using GitWorkflows.Package.VisualStudio;
using GitWorkflows.Services;
using GitWorkflows.Services.Events;
using Microsoft.VisualStudio.Shell;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandGetBranches : MenuCommand
    {
        [Import]
        private IBranchManager _branchManager;

        [Import]
        private GitBranchCollectionChangedEvent BranchCollectionChangedEvent
        {
            set { value.Subscribe(branchManager => _isEnabled = branchManager.Branches.Any()); }
        }

        private volatile bool _isEnabled;

        public CommandGetBranches()
            : base(Constants.guidPackageCmdSet, Constants.idBranchComboGetBranches)
        {}

        protected override void Execute(object sender, OleMenuCmdEventArgs e)
        {
            Marshal.GetNativeVariantForObject(_branchManager.Branches.Select(b => b.Name).ToArray(), e.OutValue);
        }

        protected override void DoUpdateStatus(object sender, EventArgs e)
        { Command.Enabled = _isEnabled; }
    }
}