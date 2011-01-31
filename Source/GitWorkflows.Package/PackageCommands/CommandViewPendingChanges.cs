using System;
using System.ComponentModel.Composition;
using GitWorkflows.Package.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandViewPendingChanges : MenuCommand
    {
        public CommandViewPendingChanges() 
            : base(Constants.guidPackageCmdSet, Constants.cmdidPendingChanges)
        {}

        protected override void Execute(object sender, OleMenuCmdEventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var window = HostPackage.FindToolWindow(typeof(PendingChangesWindow), 0, true);
            if (window == null || window.Frame == null)
                throw new NotSupportedException(Resources.CanNotCreateWindow);

            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}