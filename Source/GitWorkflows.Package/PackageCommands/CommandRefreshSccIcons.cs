using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using GitWorkflows.Package.Interfaces;
using GitWorkflows.Package.Extensions;
using GitWorkflows.Package.VisualStudio;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandRefreshSccIcons : MenuCommand
    {
        [Import]
        private SourceControlProvider _sourceControlProvider;

        [Import]
        private ISolutionService _solutionService;

        public CommandRefreshSccIcons() 
            : base(Constants.guidPackageCmdSet, Constants.cmdidRefreshSccIcons)
        {}

        protected override void Execute(object sender, OleMenuCmdEventArgs e)
        { _solutionService.RefreshSourceControlIcons(); }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            var shell = ServiceProvider.GetService<SVsUIShell, IVsUIShell>();
            
            EventHandler handler = (sender, e) => 
            {
                var guid = CommandID.Guid;
                object variant = null;
                ErrorHandler.ThrowOnFailure(
                    shell.PostExecCommand(ref guid, (uint)CommandID.ID, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref variant)
                );
            };

            _sourceControlProvider.Activated += handler;
            _sourceControlProvider.Deactivated += handler;
        }

        [DllImport("oleaut32", PreserveSig = false)]
        static extern void VariantInit(IntPtr pObject);

        [DllImport("oleaut32", PreserveSig = false)]
        static extern void VariantClear(IntPtr pObject);
    }
}