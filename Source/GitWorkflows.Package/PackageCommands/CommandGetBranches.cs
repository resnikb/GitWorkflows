using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using GitWorkflows.Git;
using GitWorkflows.Package.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandGetBranches : MenuCommand
    {
        [Import]
        private IBranchManager _branchManager;

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

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            _branchManager.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Branches")
                    _isEnabled = _branchManager.Branches.Any();                                    
            };
        }
    }
}