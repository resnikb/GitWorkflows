using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using GitWorkflows.Package.Git;
using GitWorkflows.Package.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandGetBranches : MenuCommand
    {
        [Import]
        private IBranchManager _branchManager;

        [Import]
        private IGitService _gitService;

        [ImportingConstructor]
        public CommandGetBranches(IGitService solutionService)
            : base(Constants.guidPackageCmdSet, Constants.idBranchComboGetBranches)
        {}

        protected override void Execute(object sender, EventArgs e)
        {
            if (e == null || e == EventArgs.Empty)
                return;

            var eventArgs = (OleMenuCmdEventArgs)e;
            Marshal.GetNativeVariantForObject(_branchManager.Branches.Select(b => b.Name).ToArray(), eventArgs.OutValue);
        }

        protected override void DoUpdateStatus(object sender, EventArgs e)
        { Command.Enabled = _gitService.IsRepositoryOpen; }
    }
}