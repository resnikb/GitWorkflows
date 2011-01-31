using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using GitWorkflows.Package.Interfaces;
using GitWorkflows.Package.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandGetBranches : MenuCommand
    {
        [Import]
        private IBranchManager _branchManager;

        [ImportingConstructor]
        public CommandGetBranches(IGitService solutionService)
            : base(Constants.guidPackageCmdSet, Constants.idBranchComboGetBranches)
        {}

        protected override void Execute(object sender, OleMenuCmdEventArgs e)
        {
            Marshal.GetNativeVariantForObject(_branchManager.Branches.Select(b => b.Name).ToArray(), e.OutValue);
        }

        protected override void DoUpdateStatus(object sender, EventArgs e)
        { Command.Enabled = _branchManager.Branches.Any(); }
    }
}