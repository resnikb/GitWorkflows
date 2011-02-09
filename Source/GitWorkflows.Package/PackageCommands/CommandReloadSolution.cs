using System.ComponentModel.Composition;
using GitWorkflows.Package.Interfaces;
using GitWorkflows.Package.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandReloadSolution : MenuCommand
    {
        [Import]
        private ISolutionService _solutionService;

        public CommandReloadSolution() 
            : base(Constants.guidPackageCmdSet, Constants.cmdidReloadSolution)
        {}

        protected override void Execute(object sender, OleMenuCmdEventArgs e)
        { _solutionService.Reload(); }
    }
}