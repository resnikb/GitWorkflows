using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using GitWorkflows.Package.Common;
using GitWorkflows.Package.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandGetBranches : MenuCommand
    {
        private readonly ISolutionService _solutionService;

        private readonly Cache<string[]> _branches;

        [ImportingConstructor]
        public CommandGetBranches(ISolutionService solutionService)
            : base(Constants.guidPackageCmdSet, Constants.idBranchComboGetBranches)
        {
            _solutionService = solutionService;
            _branches = new Cache<string[]>(() => solutionService.IsControlledByGit ? solutionService.WorkingTree.Branches.ToArray() : null);
            solutionService.SolutionChanged += (sender, e) => _branches.Invalidate();
            solutionService.RepositoryChanged += (sender, e) => _branches.Invalidate();
        }

        protected override void Execute(object sender, EventArgs e)
        {
            if (e == null || e == EventArgs.Empty)
                return;

            var eventArgs = (OleMenuCmdEventArgs)e;

            Marshal.GetNativeVariantForObject(_branches.Value, eventArgs.OutValue);
        }

        protected override void DoUpdateStatus(object sender, EventArgs e)
        { Command.Enabled = _solutionService.IsControlledByGit; }
    }
}